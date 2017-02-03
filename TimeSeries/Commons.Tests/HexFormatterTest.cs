using FluentAssertions;
using NUnit.Framework;

namespace Commons.Tests
{
    [TestFixture]
    public class HexFormatterTest
    {
        [Test]
        public void Test()
        {
            new byte[] {1, 2, 3}.ToHexString().Should().Be("010203");
            new byte[] {1, 0xDE, 0xFF}.ToHexString().Should().Be("01DEFF");
            new byte[] {}.ToHexString().Should().Be(string.Empty);
            HexFormatter.ToHexString(null).Should().BeNull();
        }
    }
}