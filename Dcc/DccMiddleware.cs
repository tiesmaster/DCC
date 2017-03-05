using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tiesmaster.Dcc
{
    public class DccMiddleware
    {
        private readonly DccOptions _options;
        private readonly TapeRepository _tapeRepository = new TapeRepository();
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        // ReSharper disable once UnusedParameter.Local
        public DccMiddleware(RequestDelegate next, IOptions<DccOptions> options, ILoggerFactory loggerFactory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options.Value;

            if (string.IsNullOrEmpty(_options.Host))
            {
                throw new ArgumentException("Options parameter must specify host.", nameof(options));
            }

            _httpClient = new HttpClient(_options.BackChannelMessageHandler ?? new HttpClientHandler());

            _logger = loggerFactory.CreateLogger<DccMiddleware>();
        }

        // ReSharper disable once UnusedMember.Global
        public async Task Invoke(HttpContext context)
        {
            var incomingRequest = context.Request;
            var requestKey = new RequestKey(incomingRequest);

            var tapedResponse = _tapeRepository.Get(requestKey);
            if(tapedResponse != null)
            {
                _logger.LogInformation("request was previously recorded, playing back tape");
                tapedResponse.WriteTo(context.Response);
            }
            else
            {
                _logger.LogInformation("could not find recorded tape for request, passing through, and recording it");

                var incomingResponse = await ProxyRequestAsync(incomingRequest, context.RequestAborted);

                tapedResponse = await requestKey.CreateTapeFromAsync(incomingResponse);
                tapedResponse.WriteTo(context.Response);

                _tapeRepository.Store(tapedResponse);
            }
        }

        private async Task<HttpResponseMessage> ProxyRequestAsync(HttpRequest incomingRequest, CancellationToken contextRequestAborted)
        {
            var outgoingRequest = Helpers.CreateHttpRequestMessageFrom(incomingRequest);
            RewriteDestination(outgoingRequest, incomingRequest);

            return await _httpClient.SendAsync(outgoingRequest, HttpCompletionOption.ResponseHeadersRead, contextRequestAborted);
        }

        private void RewriteDestination(HttpRequestMessage clonedRequest, HttpRequest originalRequest)
        {
            clonedRequest.RequestUri = new Uri(
                $"http://{_options.Host}:{_options.Port}{originalRequest.PathBase}{originalRequest.Path}{originalRequest.QueryString}");
            clonedRequest.Headers.Host = _options.Host + ":" + _options.Port;
        }
    }
}