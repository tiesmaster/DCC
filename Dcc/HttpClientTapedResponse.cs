using System.Linq;
using System.Net.Http;

using Microsoft.AspNetCore.Http;

namespace Tiesmaster.Dcc
{
    internal class HttpClientTapedResponse : TapedResponse
    {
        private readonly HttpResponseMessage _responseMessage;
        private readonly byte[] _body;

        public HttpClientTapedResponse(HttpResponseMessage responseMessage, byte[] body)
        {
            _responseMessage = responseMessage;
            _body = body;
        }

        public override void WriteTo(HttpResponse aspnetResponse)
        {
            aspnetResponse.StatusCode = (int)_responseMessage.StatusCode;
            foreach(var header in _responseMessage.Headers)
            {
                aspnetResponse.Headers[header.Key] = header.Value.ToArray();
            }

            foreach(var header in _responseMessage.Content.Headers)
            {
                aspnetResponse.Headers[header.Key] = header.Value.ToArray();
            }

            // TODO: find out what this means, and why it needs to be removed
            // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
            aspnetResponse.Headers.Remove("transfer-encoding");
            foreach(var b in _body)
            {
                aspnetResponse.Body.WriteByte(b);
            }
        }
    }
}