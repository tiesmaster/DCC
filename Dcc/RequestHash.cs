using Microsoft.AspNetCore.Http;

namespace Tiesmaster.Dcc
{
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