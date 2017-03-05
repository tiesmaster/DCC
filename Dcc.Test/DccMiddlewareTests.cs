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
            var listeningPort = 1234;

            var expectedResponse = Guid.NewGuid().ToString();
            var clientHandlerMock = new HttpClientHandlerMock
            {
                Sender = req =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(expectedResponse);
                    return response;
                }
            };

            var server = CreateDccTestServerWith(listeningPort, clientHandlerMock);

            // act
            var result = await server.CreateClient().GetStringAsync("test");

            // assert
            result.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task SecondInvokeWillReturnStoredTape()
        {
            // arrange
            var listeningPort = 1235;

            var invocationCount = 0;
            var clientHandlerMock = new HttpClientHandlerMock
            {
                Sender = req =>
                {
                    invocationCount++;
                    var response  = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(Guid.NewGuid().ToString());
                    return response;
                }
            };

            var server = CreateDccTestServerWith(listeningPort, clientHandlerMock);
            await server.CreateClient().GetStringAsync("test");

            // act
            await server.CreateClient().GetAsync("test");

            // assert
            invocationCount.Should().Be(1);
        }

        private static TestServer CreateDccTestServerWith(int listeningPort, HttpMessageHandler httpClientMessageHandler)
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
            public Func<HttpRequestMessage, HttpResponseMessage> Sender { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if(Sender != null)
                {
                    return Task.FromResult(Sender(request));
                }

                return Task.FromResult<HttpResponseMessage>(null);
            }
        }
    }
}