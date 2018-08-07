using Microsoft.Extensions.PlatformAbstractions;
using PeterKottas.DotNetCore.WindowsService.Interfaces;
using System;
using System.IO;
using System.Timers;
using PeterKottas.DotNetCore.WindowsService;
using Microsoft.Extensions.Options;
using Complaya;
using Serilog;  



namespace Complaya.Service
{
    public class ExampleService : IMicroService
    {
        
        private ILogger logger;
		private Timer timer = new Timer(10000);
        private KxClient client;
        private FolderWatcher folderWatcher;
               
        public ExampleService(ILogger logger, KxClient client, FolderWatcher folderWatcher, IOptions<DocumentTypeConfiguration> documentTypeConfiguration)
        {
            this.logger = logger;
            this.client = client;
            this.folderWatcher = folderWatcher;
            
            
          
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
			logger.Information($"Polling at {DateTime.Now.ToString("o")}");
            logger.Information(folderWatcher.FilesAdded.Count.ToString());
           
            foreach(var file in folderWatcher.FilesAdded){
                var documentToSend = new DocumentToSend();
                documentToSend.FileName = Path.GetFilename(file);
                using(var bytesToSend = new System.IO.ReadAllBytes(file)){
                var documentToSend = new DocumentToSend();
                documentToSend.Data = bytesToSend;
                }
                
                var result = await client.Post(documentToSend,"" );
                if(result.Success){


                }
                
                
                logger.Information($"File to process detected:{file}");
                }

                
            }
            
            //client.Post();
		

		public void Stop()
        {
			timer.Stop();
            folderWatcher.Dispose();
            logger.Information("Stopped");
           
        }
    }
}
