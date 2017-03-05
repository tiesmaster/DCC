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
            var expectedResponse = Guid.NewGuid().ToString();
            var clientHandlerMock = HttpClientHandlerMock.CreateWithExpectedResponse(HttpStatusCode.OK, expectedResponse);

            var server = CreateDccTestServerWith(clientHandlerMock, listeningPort: 1234);

            // act
            var result = await server.CreateClient().GetStringAsync("test");

            // assert
            result.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task SecondInvokeWillReturnStoredTape()
        {
            // arrange
            var invocationCount = 0;
            var clientHandlerMock = new HttpClientHandlerMock(req =>
            {
                invocationCount++;
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(Guid.NewGuid().ToString());
                return response;
            });

            var server = CreateDccTestServerWith(clientHandlerMock, listeningPort: 1235);
            await server.CreateClient().GetStringAsync("test");

            // act
            await server.CreateClient().GetAsync("test");

            // assert
            invocationCount.Should().Be(1);
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

        private static IOptions<DccOptions> CreateOptions() => Options.Create(new DccOptions {Host = "test"});
        private Task NoOpNext(HttpContext context) => Task.CompletedTask;

        private class HttpClientHandlerMock : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _sendCallback;

            public static HttpClientHandlerMock CreateWithExpectedResponse(HttpStatusCode httpStatusCode, string expectedResponse)
            {
                var response = new HttpResponseMessage(httpStatusCode) {Content = new StringContent(expectedResponse)};

                return new HttpClientHandlerMock(_ => response);
            }

            public HttpClientHandlerMock(Func<HttpRequestMessage, HttpResponseMessage> sendCallback)
            {
                _sendCallback = sendCallback;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_sendCallback(request));
        }
    }
}