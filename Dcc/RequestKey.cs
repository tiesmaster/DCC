using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Tiesmaster.Dcc
{
    // ReSharper disable once UseNameofExpression
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct RequestKey
    {
        private readonly string _method;
        private readonly PathString _path;

        public RequestKey(HttpRequest request)
        {
            _method = request.Method;
            _path = request.Path;
        }

        public async Task<TapedResponse> CreateTapeFromAsync(HttpResponseMessage httpResponseMessage)
        {
            var body = await httpResponseMessage.Content.ReadAsByteArrayAsync();
            return new HttpClientTapedResponse(this, httpResponseMessage, body);
        }

        private string DebuggerDisplay => $"{_method}: {_path}";
    }
}