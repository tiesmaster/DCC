using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Tiesmaster.Dcc
{
    public abstract class TapedResponse
    {
        public static async Task<TapedResponse> CreateFromAsync(HttpResponseMessage httpResponseMessage)
        {
            var body = await httpResponseMessage.Content.ReadAsByteArrayAsync();
            return new HttpClientTapedResponse(httpResponseMessage, body);
        }

        public abstract void WriteTo(HttpResponse aspnetResponse);
    }
}