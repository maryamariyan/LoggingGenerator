// © Microsoft Corporation. All rights reserved.

using Microsoft.Extensions.Logging.Internal;
using Xunit;

namespace Microsoft.Extensions.Logging.Test
{
    public class ArgumentFormatterTests
    {
        [Fact]
        public void Basic()
        {
            var r = ArgumentFormatter.Enumerate(null);
            Assert.Equal("(null)", r);

            r = ArgumentFormatter.Enumerate(new[] { "A", "B", "C" });
            Assert.Equal("[A, B, C]", r);

            r = ArgumentFormatter.Enumerate(new[] { "A", null, "C" });
            Assert.Equal("[A, (null), C]", r);

            r = ArgumentFormatter.Enumerate(new object[] { "A", 1, "C" });
            Assert.Equal("[A, 1, C]", r);
        }
    }
}
