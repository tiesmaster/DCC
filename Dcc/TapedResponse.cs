using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Tiesmaster.Dcc
{
    public abstract class TapedResponse
    {
        protected TapedResponse(RequestKey requestKey)
        {
            RequestKey = requestKey;
        }

        public static async Task<TapedResponse> CreateFromAsync(RequestKey requestKey, HttpResponseMessage httpResponseMessage)
        {
            var body = await httpResponseMessage.Content.ReadAsByteArrayAsync();
            return new HttpClientTapedResponse(requestKey, httpResponseMessage, body);
        }

        public abstract void WriteTo(HttpResponse aspnetResponse);
        public RequestKey RequestKey { get; }
    }
}