using Microsoft.AspNetCore.Http;

namespace Tiesmaster.Dcc
{
    public abstract class TapedResponse
    {
        protected TapedResponse(RequestKey requestKey)
        {
            RequestKey = requestKey;
        }

        public abstract void WriteTo(HttpResponse aspnetResponse);
        public RequestKey RequestKey { get; }
    }
}