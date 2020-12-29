// © Microsoft Corporation. All rights reserved.

using System;
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
        }
    }
}
