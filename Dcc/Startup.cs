using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Tiesmaster.Dcc
{
    public class Startup
    {
        private static readonly string _host = "jsonplaceholder.typicode.com";
        private static readonly int _port = 80;
        private static readonly string _scheme = "http";
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Dictionary<RequestHash, HttpResponseMessage> _tapes = new Dictionary<RequestHash, HttpResponseMessage>();

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if(env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(RunDccProxyAsync);
        }

        private static async Task RunDccProxyAsync(HttpContext context)
        {
            var incomingRequest = context.Request;
            var requestHash = new RequestHash(incomingRequest);

            var outgoingResponse = context.Response;

            HttpResponseMessage tapedResponse;
            if(_tapes.TryGetValue(requestHash, out tapedResponse))
            {
                await CloneResponseMessageToAsync(outgoingResponse, tapedResponse);
            }
            else
            {
                var outgoingRequest = CloneRequestMessage(incomingRequest);
                RewriteDestination(outgoingRequest, incomingRequest);

                var incomingResponse = await _httpClient.SendAsync(outgoingRequest, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
                _tapes[requestHash] = incomingResponse;
                await CloneResponseMessageToAsync(outgoingResponse, incomingResponse);
            }
        }

        private static HttpRequestMessage CloneRequestMessage(HttpRequest originalRequest)
        {
            var clonedRequest = new HttpRequestMessage {Method = new HttpMethod(originalRequest.Method)};

            if(CanRequestContainBody(originalRequest.Method))
            {
                clonedRequest.Content = new StreamContent(originalRequest.Body);
            }
            CloneHeaders(originalRequest, clonedRequest);

            return clonedRequest;
        }

        private static Task CloneResponseMessageToAsync(HttpResponse outgoingResponse, HttpResponseMessage incomingResponse)
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
            return incomingResponse.Content.CopyToAsync(outgoingResponse.Body);
        }

        private static void RewriteDestination(HttpRequestMessage clonedRequest, HttpRequest originalRequest)
        {
            var uriString = $"{_scheme}://{_host}:{_port}{originalRequest.PathBase}{originalRequest.Path}{originalRequest.QueryString}";

            clonedRequest.RequestUri = new Uri(uriString);
            clonedRequest.Headers.Host = _host + ":" + _port;
        }

        private static bool CanRequestContainBody(string requestMethod)
        {
            return !HttpMethods.IsGet(requestMethod) &&
                   !HttpMethods.IsHead(requestMethod) &&
                   !HttpMethods.IsDelete(requestMethod) &&
                   !HttpMethods.IsTrace(requestMethod);
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

    internal struct RequestHash
    {
        private readonly string _method;
        private readonly PathString _path;

        public RequestHash(HttpRequest request)
        {
            _method = request.Method;
            _path = request.Path;
        }
    }
}