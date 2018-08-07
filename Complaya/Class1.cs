using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Polly;
using Serilog;
using System.IO;

namespace Complaya
{
    public static class Bootstrap
    {
        public static void Configure(ServiceCollection services)
        {
            
            services.AddTransient<SerilogHttpMessageHandler>();

            // Register a HTTP Client
            services.AddHttpClient<KxClient>().AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                })).AddHttpMessageHandler<SerilogHttpMessageHandler>();

            
        }
    }

    class KxClient
    {
        public const string JsonDocumentType = "application/json";
        HttpClient HttpClient { get; }
        public KxClient(HttpClient client, HttpClientConfiguration config, Serilog.ILogger logger)
        {
            HttpClient = client;
            HttpClient.BaseAddress = new Uri(config.BaseAddress);
            foreach (var header in config.DefaultRequestHeaders)
            {
                HttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

        }
        public async Task<HttpResponseMessage> Get(string resource, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new HttpRequestMessage(HttpMethod.Get, resource);
            var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return response;
        }

        public async Task<ParsedDocument> Post(DocumentToSend documentToSend, string resource, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new HttpRequestMessage(HttpMethod.Post, resource);
            var jsonString = JsonConvert.SerializeObject(documentToSend);
            request.Content = new StringContent(jsonString, Encoding.UTF8, JsonDocumentType);
            var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<ParsedDocument>(await response.Content.ReadAsStringAsync());

        }

    }

    public class HttpClientConfiguration
    {
        public string BaseAddress { get; set; }
        public bool ValidateCertificate { get; set; }

        public Dictionary<string, IEnumerable<String>> DefaultRequestHeaders { get; set; } = new Dictionary<string, IEnumerable<string>>();
    }

    public class DocumentToSend
    {
        public string FileName { get; set; }
        public byte[] Data { get; set; }
    }

    public class ParsedDocument
    {
        public string DocumentType { get; set; }
        public string Result { get; set; }
        public string Data { get; set; }
    }

    public class SerilogHttpMessageHandler : DelegatingHandler
    {
        private Serilog.ILogger logger;

        public SerilogHttpMessageHandler(Serilog.ILogger logger)
        {
            this.logger = logger;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            this.logger.Debug("Request completed: {route} {method} {code} {headers}", request.RequestUri.Host, request.Method, response.StatusCode, request.Headers);
            return response;
        }
    }

    public class FolderWatcherConfiguration
    {
        public string Path { get; set; }
        public string Filter { get; set; } = "*.pdf";

    }

    public class FolderWatcher : IDisposable
    {
        public FileSystemWatcher FileWatcher { get; set; }
        public FolderWatcherConfiguration Configuration { get; set; }

        public List<string> FilesAdded { get; set; } = new List<string>();

        public FolderWatcher(FolderWatcherConfiguration configuration, Serilog.ILogger logger)
        {
            FileWatcher = new FileSystemWatcher();
            Configuration = configuration;

            this.logger = logger;
            var filter = configuration.Filter;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                FileWatcher.Filter = filter;
            }

            var path = configuration.Path;
            if (!string.IsNullOrWhiteSpace(path))
            {
                FileWatcher.Path = path;
            }

            FileWatcher.Created += Created;
            FileWatcher.Deleted += Deleted;

        }

        private void Deleted(object sender, FileSystemEventArgs e)
        {
            if (FilesAdded.Remove(e.FullPath))
            {
                logger.Information($"File removed {e.Name}.");
            }

        }

        private void Created(object sender, FileSystemEventArgs e)
        {
            FilesAdded.Add(e.FullPath);
            logger.Information($"New file added {e.Name}.");
        }

        public void AddNotifyFilter(NotifyFilters notifyFilters)
        {
            FileWatcher.NotifyFilter = notifyFilters;
        }
        private readonly Serilog.ILogger logger;


        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    FileWatcher.Dispose();
                    FileWatcher = null;

                }
                disposedValue = true;
            }
        }


        public void Dispose()
        {

            Dispose(true);

        }
        #endregion


    }


}
