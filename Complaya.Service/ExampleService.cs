using Microsoft.Extensions.PlatformAbstractions;
using PeterKottas.DotNetCore.WindowsService.Interfaces;
using System;
using System.IO;
using System.Timers;
using PeterKottas.DotNetCore.WindowsService;
using Complaya;
using Serilog;  


namespace Complaya.Service
{
    public class ExampleService : IMicroService
    {
        
        private ILogger logger;

		private Timer timer = new Timer(1000);
        private KxClient client;
       
        public ExampleService(ILogger logger, KxClient client)
        {
            this.logger = logger;
            this.client = client;
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
		}

		public void Stop()
        {
			timer.Stop();
            logger.Information("Stopped");
           
        }
    }
}
