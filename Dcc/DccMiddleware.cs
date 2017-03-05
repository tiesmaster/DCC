using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tiesmaster.Dcc
{
    public class DccMiddleware
    {
        private readonly HttpClient _httpClient;
        private readonly DccOptions _options;
        // ReSharper disable once InconsistentNaming
        // TODO: fix resharper settings
        private const string _scheme = "http";

        private static readonly Dictionary<RequestHash, Tuple<HttpResponseMessage, byte[]>> _tapes =
            new Dictionary<RequestHash, Tuple<HttpResponseMessage, byte[]>>();

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

        public async Task Invoke(HttpContext context)
        {
            var incomingRequest = context.Request;
            var requestHash = new RequestHash(incomingRequest);

            var outgoingResponse = context.Response;

            Tuple<HttpResponseMessage, byte[]> tapedTuple;
            if(_tapes.TryGetValue(requestHash, out tapedTuple))
            {
                _logger.LogInformation("request was previously recorded, playing back tape");

                var tapedResponse = tapedTuple.Item1;
                var tapedBody = tapedTuple.Item2;
                CloneResponseMessageTo(outgoingResponse, tapedResponse, tapedBody);
            }
            else
            {
                _logger.LogInformation("could not find recorded tape for request, passing through, and recording it");

                var outgoingRequest = CloneRequestMessage(incomingRequest);
                RewriteDestination(outgoingRequest, incomingRequest);

                var incomingResponse = await _httpClient.SendAsync(outgoingRequest, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
                var body = await incomingResponse.Content.ReadAsByteArrayAsync();
                _tapes[requestHash] = Tuple.Create(incomingResponse, body);
                CloneResponseMessageTo(outgoingResponse, incomingResponse, body);
            }
        }

        private static HttpRequestMessage CloneRequestMessage(HttpRequest originalRequest)
        {
            var clonedRequest = new HttpRequestMessage { Method = new HttpMethod(originalRequest.Method) };

            if(Helpers.CanRequestContainBody(originalRequest.Method))
            {
                clonedRequest.Content = new StreamContent(originalRequest.Body);
            }
            CloneHeaders(originalRequest, clonedRequest);

            return clonedRequest;
        }

        private static void CloneResponseMessageTo(HttpResponse outgoingResponse, HttpResponseMessage incomingResponse, byte[] body)
        {
            outgoingResponse.StatusCode = (int)incomingResponse.StatusCode;
            foreach(var header in incomingResponse.Headers)
            {
                outgoingResponse.Headers[header.Key] = header.Value.ToArray();
            }

            foreach(var header in incomingResponse.Content.Headers)
            {
                outgoingResponse.Headers[header.Key] = header.Value.ToArray();
            }

            // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
            outgoingResponse.Headers.Remove("transfer-encoding");
            foreach(var b in body)
            {
                outgoingResponse.Body.WriteByte(b);
            }
        }

        private void RewriteDestination(HttpRequestMessage clonedRequest, HttpRequest originalRequest)
        {
            var uriString = $"{_scheme}://{_options.Host}:{_options.Port}{originalRequest.PathBase}{originalRequest.Path}{originalRequest.QueryString}";

            clonedRequest.RequestUri = new Uri(uriString);
            clonedRequest.Headers.Host = _options.Host + ":" + _options.Port;
        }

        private static void CloneHeaders(HttpRequest originalRequest, HttpRequestMessage clonedRequestMessage)
        {
            foreach(var header in originalRequest.Headers)
            {
                if(!clonedRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    clonedRequestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }
        }
    }
}