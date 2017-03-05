using Microsoft.AspNetCore.Http;

namespace Tiesmaster.Dcc
{
    internal struct RequestKey
    {
        private readonly string _method;
        private readonly PathString _path;

        public RequestKey(HttpRequest request)
        {
            _method = request.Method;
            _path = request.Path;
        }
    }
}