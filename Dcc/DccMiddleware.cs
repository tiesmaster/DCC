using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tiesmaster.Dcc
{
    public class DccMiddleware
    {
        private readonly DccOptions _options;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<RequestKey, TapedResponse> _tapes = new Dictionary<RequestKey, TapedResponse>();
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

            TapedResponse tapedResponse;
            if(_tapes.TryGetValue(requestKey, out tapedResponse))
            {
                _logger.LogInformation("request was previously recorded, playing back tape");
                tapedResponse.WriteTo(context.Response);
            }
            else
            {
                _logger.LogInformation("could not find recorded tape for request, passing through, and recording it");

                var outgoingRequest = Helpers.CreateHttpRequestMessageFrom(incomingRequest);
                RewriteDestination(outgoingRequest, incomingRequest);

                var incomingResponse = await _httpClient.SendAsync(
                    outgoingRequest, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

                tapedResponse = await TapedResponse.CreateFromAsync(incomingResponse);
                tapedResponse.WriteTo(context.Response);

                _tapes[requestKey] = tapedResponse;
            }
        }

        private void RewriteDestination(HttpRequestMessage clonedRequest, HttpRequest originalRequest)
        {
            var uriString = $"http://{_options.Host}:{_options.Port}{originalRequest.PathBase}{originalRequest.Path}{originalRequest.QueryString}";

            clonedRequest.RequestUri = new Uri(uriString);
            clonedRequest.Headers.Host = _options.Host + ":" + _options.Port;
        }
    }
}