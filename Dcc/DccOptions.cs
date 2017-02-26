using System.Net.Http;

namespace Tiesmaster.Dcc
{
    public class DccOptions
    {
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public HttpMessageHandler BackChannelMessageHandler { get; set; }
    }
}