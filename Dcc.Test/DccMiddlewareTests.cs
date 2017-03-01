using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
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

        private static IOptions<DccOptions> CreateOptions() => Options.Create(new DccOptions {Host = "test"});
        private Task NoOpNext(HttpContext context) => Task.CompletedTask;
    }
}