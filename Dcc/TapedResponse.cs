using System.Net.Http;

namespace Tiesmaster.Dcc
{
    public class TapedResponse
    {
        public HttpResponseMessage ResponseMessage { get; }
        public byte[] Body { get; }

        public TapedResponse(HttpResponseMessage responseMessage, byte[] body)
        {
            ResponseMessage = responseMessage;
            Body = body;
        }
    }
}