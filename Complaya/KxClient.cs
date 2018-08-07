using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

namespace Complaya
{
    public class KxClient
    {
        public const string JsonDocumentType = "application/json";
        HttpClient HttpClient { get; }
        public KxClient(HttpClient client,IOptions<HttpClientConfiguration> config, Serilog.ILogger logger)
        {
            HttpClient = client;
            HttpClient.BaseAddress = new Uri(config.Value.BaseAddress);
            // foreach (var header in config.DefaultRequestHeaders)
            // {
            //     HttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            // }

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


}
