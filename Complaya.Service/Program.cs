
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.IO;
using Serilog;
using Complaya;
using PeterKottas.DotNetCore.WindowsService.Base;
using PeterKottas.DotNetCore.WindowsService;
using PeterKottas.DotNetCore.WindowsService.Interfaces;


namespace Complaya.Service
{
	public class Program
    {
        public static void Main(string[] args)
        {
            #if !DEBUG
            var configuration = new ConfigurationBuilder()
                .SetBasePath(PlatformServices.Default.Application.ApplicationBasePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
#else
            var configuration = new ConfigurationBuilder()
                .SetBasePath(PlatformServices.Default.Application.ApplicationBasePath)
                .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
                .Build();
#endif
            var fileName = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "log.txt");
            ConfigureServices(configuration);

            ServiceRunner<ExampleService>.Run(config =>
            {
                var name = config.GetDefaultName();
                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((extraArguments, controller) =>
                    {
                        return new ExampleService(controller, Log.Logger);
                    });

                    serviceConfig.OnStart((service, extraParams) =>
                    {
                        Log.Logger.Information("Service {0} started", name);
                        service.Start();
                    });

                    serviceConfig.OnStop(service =>
                    {
                        Log.Logger.Information("Service {0} stopped", name);
                        service.Stop();
                    });

                    serviceConfig.OnShutdown(service =>
                    {
                        Log.Logger.Information("Service {0} shutdown", name);
                        //File.AppendAllText(fileName, $"Service {name} shutdown\n");
                    });

                    serviceConfig.OnError(e =>
                    {
                        //File.AppendAllText(fileName, $"Exception: {e.ToString()}\n");
                        Log.Logger.Error("Service {0} errored with exception : {1}", name, e.Message);
                    });
                });
            });
        }

        public static void ConfigureServices(IConfiguration configuration){
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration)
                .MinimumLevel.Debug()
                .CreateLogger();
            //logger = Logger;

            var services = new ServiceCollection();
            services.AddLogging(builder =>
                {
                    builder.AddSerilog();
                });

            Complaya.Bootstrap.Configure(services);
        }

        public static Serilog.ILogger logger;
    }
}
