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

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.RunDcc(
                        new DccOptions
                        {
                            Host = "localhost",
                            Port = "1234",
                            BackChannelMessageHandler = new TestMessageHandler
                                                        {
                                                            Sender = req =>
                                                            {
                                                                var response = new HttpResponseMessage(HttpStatusCode.OK);
                                                                response.Content = new StringContent(expectedResponse);
                                                                return response;
                                                            }
                                                        }
                        });

                });
            var server = new TestServer(builder);

            // act
            var result = await server.CreateClient().GetStringAsync("test");

            // assert
            result.Should().Be(expectedResponse);
        }
        [Fact]
        public async Task SecondInvokeWillReturnStoredTape()
        {
            // arrange
            var expectedResponse = Guid.NewGuid().ToString();
            var invocationCount = 0;

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.RunDcc(
                        new DccOptions
                        {
                            Host = "localhost",
                            Port = "1235",
                            BackChannelMessageHandler = new TestMessageHandler
                            {
                                Sender = req =>
                                {
                                    invocationCount++;
                                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                                    response.Content = new StringContent(expectedResponse);
                                    return response;
                                }
                            }
                        });

                });
            var server = new TestServer(builder);
            await server.CreateClient().GetStringAsync("test");

            // act
            await server.CreateClient().GetAsync("test");

            // assert
            invocationCount.Should().Be(1);
        }

        private static IOptions<DccOptions> CreateOptions() => Options.Create(new DccOptions {Host = "test"});
        private Task NoOpNext(HttpContext context) => Task.CompletedTask;

        private class TestMessageHandler : HttpMessageHandler
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