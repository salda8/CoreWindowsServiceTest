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

namespace Complaya.Service
{
    public class ExampleService : IMicroService
    {

        private ILogger logger;
        private Timer timer = new Timer(10000);
        private KxClient client;
        private FolderWatcher folderWatcher;
        private readonly IVirtualUserConnector virtualUserConnector;
        private readonly IBaseMongoRepository repository;
        private readonly HashSet<string> typesToArchive;
        private readonly HashSet<string> typesToSendToVu;
        private readonly DocumentTypeConfiguration documentTypeConfig;

        public ExampleService(ILogger logger, KxClient client, FolderWatcher folderWatcher, IOptions<DocumentTypeConfiguration> documentTypeConfiguration, IVirtualUserConnector virtualUserConnector, IBaseMongoRepository repository)
        {
            this.logger = logger;
            this.client = client;
            this.folderWatcher = folderWatcher;
            this.virtualUserConnector = virtualUserConnector;
            this.repository = repository;
            this.documentTypeConfig = documentTypeConfiguration.Value;
            typesToArchive = documentTypeConfig.ToArchive;
            typesToSendToVu = documentTypeConfig.ToSendToVirtualUser;
        }

        //private string fileName = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "log.txt");
        public void Start()
        {

            logger.Information("Started");

            /**
             * A timer is a simple example. But this could easily 
             * be a port or messaging queue client
             */
            timer.Elapsed += _timer_Elapsed;
            timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            logger.Information($"Starting file processsing at {DateTime.Now.ToString("o")}");
            logger.Information($"Detected {folderWatcher.FilesAdded.Count.ToString()} to process.");
            virtualUserConnector.Connect();

            Parallel.ForEach(folderWatcher.FilesAdded, async (pathToFile) => await SendAndProcessDocument(pathToFile));
            logger.Information($"File processsing stopped");
            virtualUserConnector.Disconnect();

        }

        private async Task SendAndProcessDocument(string pathToFile)
        {
            var documentToSend = new DocumentToSend();
            documentToSend.FileName = Path.GetFileName(pathToFile);
            var bytesToSend = await File.ReadAllBytesAsync(pathToFile);
            documentToSend.Data = bytesToSend;
            bool archivingSuccess = false;
            bool vuSuccess = false;

            var result = await client.Post(documentToSend, "");
            if (result.Success)
            {
                if (typesToArchive.Contains(result.DocumentType))
                {
                    await repository.AddOneAsync(new DocumentToArchive() { Data = bytesToSend });
                    logger.Information($"File {documentToSend.FileName} was successfully archived.");
                    archivingSuccess = true;
                }

                if (typesToSendToVu.Contains(result.DocumentType))
                {
                    if (!await virtualUserConnector.SendInput(result.Data))
                    {
                        logger.Error($"File {documentToSend.FileName} was not successfully processed by Virtual User.");
                    };
                    vuSuccess = true;
                }

                DeleteFileIfSuccess(archivingSuccess, vuSuccess, pathToFile);

            }
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
            timer.Stop();
            folderWatcher.Dispose();
            logger.Information("Stopped");

        }
    }
}
