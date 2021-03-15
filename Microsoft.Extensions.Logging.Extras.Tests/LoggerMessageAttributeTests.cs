// © Microsoft Corporation. All rights reserved.

using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Extensions.Logging.Test
{
    public class LoggerMessageAttributeTests
    {
        [Fact]
        public void Basic()
        {
            var a = new LoggerMessageAttribute(42, LogLevel.Trace, "Foo");
            Assert.Equal(42, a.EventId);
            Assert.Equal(LogLevel.Trace, a.Level);
            Assert.Equal("Foo", a.Message);
            Assert.Null(a.EventName);

            a.EventName = "Name";
            Assert.Equal("Name", a.EventName);

            a = new LoggerMessageAttribute(42, "Foo");
            Assert.Equal(42, a.EventId);
            Assert.False(a.Level.HasValue);
            Assert.Equal("Foo", a.Message);
            Assert.Null(a.EventName);

            a.EventName = "Name";
            Assert.Equal("Name", a.EventName);
        }
    }
}
