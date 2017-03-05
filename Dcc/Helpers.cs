using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Http;

namespace Tiesmaster.Dcc
{
    internal static class Helpers
    {
        internal static HttpRequestMessage CreateHttpRequestMessageFrom(HttpRequest originalRequest)
        {
            var clonedRequest = new HttpRequestMessage {Method = new HttpMethod(originalRequest.Method)};

            if(CanRequestContainBody(originalRequest.Method))
            {
                clonedRequest.Content = new StreamContent(originalRequest.Body);
            }
            CopyHeadersTo(originalRequest.Headers, clonedRequest);

            return clonedRequest;
        }

        private static void CopyHeadersTo(IHeaderDictionary headerDictionary, HttpRequestMessage clonedRequestMessage)
        {
            foreach(var header in headerDictionary)
            {
                if(!clonedRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    clonedRequestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }
        }

        internal static void CopyHeadersTo(HttpResponse aspnetResponse, HttpHeaders httpClientHeaders)
        {
            foreach(var header in httpClientHeaders)
            {
                aspnetResponse.Headers[header.Key] = header.Value.ToArray();
            }
        }

        private static bool CanRequestContainBody(string requestMethod)
        {
            return !HttpMethods.IsGet(requestMethod) &&
                   !HttpMethods.IsHead(requestMethod) &&
                   !HttpMethods.IsDelete(requestMethod) &&
                   !HttpMethods.IsTrace(requestMethod);
        }
    }
}