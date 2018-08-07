using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Complaya
{
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
            logger.Debug("Request completed: {route} {method} {code} {headers}", request.RequestUri.Host, request.Method, response.StatusCode, request.Headers);
            return response;
        }
    }


}
