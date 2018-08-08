
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
using Microsoft.Extensions.Options;
using MongoDbGenericRepository;

namespace Complaya.Service
{
    public class Program
    {
        static IConfiguration Configuration;
        public static void Main(string[] args)
        {
#if !DEBUG
            Configuration = new ConfigurationBuilder()
                .SetBasePath(PlatformServices.Default.Application.ApplicationBasePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
#else
            Configuration = new ConfigurationBuilder()
                .SetBasePath(PlatformServices.Default.Application.ApplicationBasePath)
                .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
                .Build();
#endif
            ConfigureServices();

            ServiceRunner<ExampleService>.Run(config =>
            {
                var vuConnector = serviceProvider.GetRequiredService<IVirtualUserConnector>();
                var kxClient = serviceProvider.GetRequiredService<KxClient>();
                var repository = serviceProvider.GetRequiredService<IBaseMongoRepository>();
                var documentTypeConfig = serviceProvider.GetRequiredService<IOptions<DocumentTypeConfiguration>>();
                var name = config.GetDefaultName();
                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((extraArguments, controller) =>
                    {
                        return new ExampleService(log, kxClient, serviceProvider.GetRequiredService<IFolderWatcher>(), documentTypeConfig, vuConnector, repository);
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
                        
                    });

                    serviceConfig.OnError(e =>
                    {
                        
                        log.Error("Service {0} errored with exception : {1}", name, e.Message);
                    });
                });
            });
        }

        public static void ConfigureServices()
        {
            log = new LoggerConfiguration().ReadFrom.Configuration(Configuration).CreateLogger();

            services.AddLogging(builder =>
                {
                    builder.AddSerilog();
                });
            services.Configure<HttpClientConfiguration>(Configuration.GetSection("HttpClient"));
            services.Configure<FolderWatcherConfiguration>(Configuration.GetSection("FolderWatcher"));
            services.Configure<DocumentTypeConfiguration>(Configuration.GetSection("DocumentTypes"));
            services.Configure<MongoConfigurationOptions>(Configuration.GetSection("MongoDatabase"));
            services.AddOptions();
            services.AddSingleton<Serilog.ILogger>(log);
            services.AddServices();
            serviceProvider = services.BuildServiceProvider();


        }
        private static Serilog.ILogger log;
        private static IServiceCollection services = new ServiceCollection();
        private static IServiceProvider serviceProvider;


    }
}
