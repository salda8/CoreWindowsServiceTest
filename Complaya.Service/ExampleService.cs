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
        private IMicroServiceController controller=null;
        private ILogger logger;

		private Timer timer = new Timer(1000);
       
        public ExampleService(IMicroServiceController controller, ILogger logger)
        {
            this.logger = logger;
            this.controller = controller;
        }

        private string fileName = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "log.txt");
        public void Start()
        {
          
            logger.Information("Started\n");

            /**
             * A timer is a simple example. But this could easily 
             * be a port or messaging queue client
             */ 
			timer.Elapsed += _timer_Elapsed;
			timer.Start();
        }

		private void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			logger.Information(fileName, string.Format("Polling at {0}\n", DateTime.Now.ToString("o")));
		}

		public void Stop()
        {
			timer.Stop();
            logger.Information("Stopped\n");
           
        }
    }
}
