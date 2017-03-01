using FluentAssertions;

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
    }
}