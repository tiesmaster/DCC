using FluentAssertions;

using Tiesmaster.Dcc;

using Xunit;

namespace Dcc.Test
{
    public class DccMiddlewareTests
    {
        [Fact]
        public void SanityTest()
        {
            true.Should().BeTrue();
        }

        [Fact]
        public void TEST_NAME()
        {
            // arrange
            var sut = new DccMiddleware(null, null);

            // act

            // assert
        }

    }
}