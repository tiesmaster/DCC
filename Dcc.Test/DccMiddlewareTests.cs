using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Options;

using Tiesmaster.Dcc;

using Xunit;

namespace Dcc.Test
{
    public class DccMiddlewareTests
    {
        [Fact]
        public void CanCreate()
        {
            // arrange
            var options = CreateOptions();

            // act
            var sut = new DccMiddleware(NoOpNext, options);

            // assert
            sut.Should().NotBeNull();
        }

        [Fact]
        public async Task FirstInvokeWillProxyRequest()
        {
            // arrange
            var expectedResponse = CreateAnonymousString();
            var clientHandlerMock = HttpClientHandlerMock.CreateWithExpectedResponse(HttpStatusCode.OK, expectedResponse);

            var server = CreateDccTestServerWith(clientHandlerMock, listeningPort: 1234);

            // act
            var result = await server.CreateClient().GetStringAsync("test-endpoint");

            // assert
            result.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task SecondInvokeWillReturnStoredTape()
        {
            // arrange
            var clientHandlerMock = HttpClientHandlerMock.CreateWithExpectedResponse(HttpStatusCode.OK, CreateAnonymousString());

            var server = CreateDccTestServerWith(clientHandlerMock, listeningPort: 1235);
            await server.CreateClient().GetStringAsync("test-endpoint");

            // act
            await server.CreateClient().GetAsync("test-endpoint");

            // assert
            clientHandlerMock.SendCallbackInvocationCount.Should().Be(1);
        }

        private static TestServer CreateDccTestServerWith(HttpMessageHandler httpClientMessageHandler, int listeningPort)
        {
            var dccOptions = new DccOptions
            {
                Host = "localhost",
                Port = listeningPort.ToString(),
                BackChannelMessageHandler = httpClientMessageHandler
            };

            var builder = new WebHostBuilder().Configure(app => app.RunDcc(dccOptions));
            return new TestServer(builder);
        }

        private Task NoOpNext(HttpContext context) => Task.CompletedTask;
        private static string CreateAnonymousString() => Guid.NewGuid().ToString();
        private static IOptions<DccOptions> CreateOptions() => Options.Create(new DccOptions {Host = "localhost"});

        private class HttpClientHandlerMock : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _sendCallback;

            public static HttpClientHandlerMock CreateWithExpectedResponse(HttpStatusCode httpStatusCode, string expectedResponse)
            {
                var response = new HttpResponseMessage(httpStatusCode) {Content = new StringContent(expectedResponse)};

                return new HttpClientHandlerMock(_ => response);
            }

            public int SendCallbackInvocationCount { get; private set; }

            private HttpClientHandlerMock(Func<HttpRequestMessage, HttpResponseMessage> sendCallback)
            {
                _sendCallback = sendCallback;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                SendCallbackInvocationCount++;
                return Task.FromResult(_sendCallback(request));
            }
        }
    }
}