using System.Net.Http;

// TODO: consider moving this to Tiesmaster.Dcc.Builder

namespace Tiesmaster.Dcc
{
    public class DccOptions
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public HttpMessageHandler BackChannelMessageHandler { get; set; }
    }
}