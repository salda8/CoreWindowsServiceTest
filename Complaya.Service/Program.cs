
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
            //var fileName = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "log.txt");
            ConfigureServices(configuration);

            ServiceRunner<ExampleService>.Run(config =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var config = serviceProvider.GetRequiredService<DocumentTypeConfiguration>();
               
                var kxClient = serviceProvider.GetRequiredService<KxClient>();
                var name = config.GetDefaultName();
                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((extraArguments, controller) =>
                    {
                        return new ExampleService(log, kxClient, serviceProvider.GetRequiredService<FolderWatcher>(),Configuration.GetSection("DocumentTypes").Get<DocumentTypeConfiguration>());
                    });

                    serviceConfig.OnStart((service, extraParams) =>
                    {
                        log.Information("Service {0} started", name);
                        service.Start();
                    });

                    serviceConfig.OnStop(service =>
                    {
                        log.Information("Service {0} stopped", name);
                        service.Stop();
                    });

                    serviceConfig.OnShutdown(service =>
                    {
                        log.Information("Service {0} shutdown", name);
                        //File.AppendAllText(fileName, $"Service {name} shutdown\n");
                    });

                    serviceConfig.OnError(e =>
                    {
                        //File.AppendAllText(fileName, $"Exception: {e.ToString()}\n");
                        log.Error("Service {0} errored with exception : {1}", name, e.Message);
                    });
                });
            });
        }

        public static void ConfigureServices(IConfiguration configuration){
            log = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
                
            services.AddLogging(builder =>
                {
                    builder.AddSerilog();
                });
            services.Configure<HttpClientConfiguration>(configuration.GetSection("HttpClient"));
            services.Configure<FolderWatcherConfiguration>(configuration.GetSection("FolderWatcher"));
            services.Configure<DocumentTypeConfiguration>(configuration.GetSection("DocumentTypes"));

            services.AddSingleton<Serilog.ILogger>(log);
            services.AddServices();
           

        }
        private static Serilog.ILogger log;
         private static IServiceCollection services = new ServiceCollection();
         

    }
}
