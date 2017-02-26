using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Tiesmaster.Dcc
{
    public class DccMiddleware
    {
        private readonly HttpClient _httpClient;
        private readonly DccOptions _options;

        public DccMiddleware(RequestDelegate next, IOptions<DccOptions> options)
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

            // Setting default Port and Scheme if not specified
            if (string.IsNullOrEmpty(_options.Port))
            {
                if (string.Equals(_options.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                {
                    _options.Port = "443";
                }
                else
                {
                    _options.Port = "80";
                }

            }

            if (string.IsNullOrEmpty(_options.Scheme))
            {
                _options.Scheme = "http";
            }

            _httpClient = new HttpClient(_options.BackChannelMessageHandler ?? new HttpClientHandler());
        }

        public async Task Invoke(HttpContext context)
        {
            var requestMessage = new HttpRequestMessage();
            var requestMethod = context.Request.Method;
            if (!HttpMethods.IsGet(requestMethod)&&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod)&&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            // Copy the request headers
            foreach (var header in context.Request.Headers)
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            requestMessage.Headers.Host = _options.Host + ":" + _options.Port;
            var uriString = $"{_options.Scheme}://{_options.Host}:{_options.Port}{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
            requestMessage.RequestUri = new Uri(uriString);
            requestMessage.Method = new HttpMethod(context.Request.Method);
            using (var responseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
            {
                context.Response.StatusCode = (int)responseMessage.StatusCode;
                foreach (var header in responseMessage.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                foreach (var header in responseMessage.Content.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
                context.Response.Headers.Remove("transfer-encoding");
                await responseMessage.Content.CopyToAsync(context.Response.Body);
            }
        }
    }
}