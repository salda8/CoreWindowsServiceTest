using Microsoft.Extensions.PlatformAbstractions;
using PeterKottas.DotNetCore.WindowsService.Interfaces;
using System;
using System.IO;
using System.Timers;
using PeterKottas.DotNetCore.WindowsService;
using Microsoft.Extensions.Options;
using Complaya;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDbGenericRepository;
using System.Linq;
using FluentScheduler;

namespace Complaya.Service
{
    public class ExampleService : IMicroService, IJob
    {

        private ILogger logger;
        private KxClient client;
        private IFolderWatcher folderWatcher;
        private readonly IVirtualUserConnector virtualUserConnector;
        private readonly IBaseMongoRepository repository;
        private readonly ProcessDocumentConfiguration timerconfig;
        private readonly HashSet<string> typesToArchive;
        private readonly HashSet<string> typesToSendToVu;
        private readonly DocumentTypeConfiguration documentTypeConfig;

        public ExampleService(ILogger logger, KxClient client, IFolderWatcher folderWatcher, IOptions<DocumentTypeConfiguration> documentTypeConfiguration, IVirtualUserConnector virtualUserConnector, IBaseMongoRepository repository, IOptions<ProcessDocumentConfiguration> timerconfig)
        {
            this.logger = logger;
            this.client = client;
            this.folderWatcher = folderWatcher;
            this.virtualUserConnector = virtualUserConnector;
            this.repository = repository;
            this.timerconfig = timerconfig.Value;
            this.documentTypeConfig = documentTypeConfiguration.Value;
            typesToArchive = documentTypeConfig.ToArchive;
            typesToSendToVu = documentTypeConfig.ToSendToVirtualUser;
        }

        public void Start()
        {
            if (timerconfig.RunOnceImmediately)
            {
                JobManager.AddJob(Execute, s => s.ToRunNow());
            }
            if (timerconfig.RunEveryXSec != 0)
            {
                JobManager.AddJob(Execute, s => s.ToRunEvery(timerconfig.RunEveryXSec).Seconds());
            }
            else
            {
                if (timerconfig.RunAt.Hour != 0 || (timerconfig.RunAt.Hour == 0 && timerconfig.RunAt.Minute != 0))
                {
                    JobManager.AddJob(Execute, s => s.ToRunOnceAt(timerconfig.RunAt.Hour, timerconfig.RunAt.Minute));
                }
                else
                {
                    throw new InvalidProcessDocumentConfigurationException();
                }
            }

        }

        public void Execute()
        {
            logger.Information($"Starting file processsing at {DateTime.Now.ToString("o")}");
            logger.Information($"Detected {folderWatcher.FilesAdded.Count().ToString()} files to process.");
            virtualUserConnector.Connect();

            Parallel.ForEach(folderWatcher.FilesAdded, async (pathToFile) => await SendAndProcessDocument(pathToFile));
            logger.Information($"File processsing stopped");
            virtualUserConnector.Disconnect();
        }

        private async Task SendAndProcessDocument(string pathToFile)
        {

            var documentToSend = new DocumentToSend();
            documentToSend.FileName = Path.GetFileName(pathToFile);
            logger.Debug($"Processing of file:{Path.GetFileName(pathToFile)} started");
            var bytesToSend = await File.ReadAllBytesAsync(pathToFile);
            documentToSend.Data = bytesToSend;
            bool archivingSuccess = false;
            bool vuSuccess = false;

            var result = await client.Post(documentToSend, "");
            if (result.Success)
            {
                archivingSuccess = await SaveToArchive(documentToSend, bytesToSend, archivingSuccess, result);

                vuSuccess = await SendToVirtualUser(documentToSend, vuSuccess, result);

                DeleteFileIfSuccess(archivingSuccess, vuSuccess, pathToFile);
                logger.Debug($"Processing of file:{documentToSend.FileName} was successful");

            }
        }

        private async Task<bool> SaveToArchive(DocumentToSend documentToSend, byte[] bytesToSend, bool archivingSuccess, ParsedDocument result)
        {
            if (typesToArchive.Contains(result.DocumentType))
            {
                await repository.AddOneAsync(new DocumentToArchive() { Data = bytesToSend });
                logger.Information($"File {documentToSend.FileName} was successfully archived");
                archivingSuccess = true;
            }

            return archivingSuccess;
        }

        private async Task<bool> SendToVirtualUser(DocumentToSend documentToSend, bool vuSuccess, ParsedDocument result)
        {
            if (typesToSendToVu.Contains(result.DocumentType))
            {
                if (!await virtualUserConnector.SendInput(result.Data))
                {
                    logger.Error($"File {documentToSend.FileName} was not successfully processed by Virtual User");
                };
                logger.Information($"File {documentToSend.FileName} was processed by Virtual User");
                vuSuccess = true;
            }

            return vuSuccess;
        }


        private void DeleteFileIfSuccess(bool archivingSuccess, bool vuSuccess, string pathToFile)
        {
            if (archivingSuccess && vuSuccess)
            {
                File.Delete(pathToFile);
            }
        }

        public void Stop()
        {
            JobManager.StopAndBlock();
            logger.Information("Stopped");
        }

    }
}
