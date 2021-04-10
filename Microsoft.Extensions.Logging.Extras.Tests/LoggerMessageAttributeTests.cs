// © Microsoft Corporation. All rights reserved.

using Xunit;

namespace Microsoft.Extensions.Logging.Test
{
    public class LoggerMessageAttributeTests
    {
        [Fact]
        public void Basic()
        {
            var a = new LoggerMessageAttribute()
            {
                Level = LogLevel.Trace, 
                EventId = 42, 
                Message = "Foo"
            };
            Assert.Equal(42, a.EventId);
            Assert.Equal(LogLevel.Trace, a.Level);
            Assert.Equal("Foo", a.Message);
            Assert.Null(a.EventName);

            a.EventName = "Name";
            Assert.Equal("Name", a.EventName);

            a = new LoggerMessageAttribute()
            {
                EventId = 42,
                Message = "Foo"
            };
            Assert.Equal(42, a.EventId);
            Assert.Equal(LogLevel.None, a.Level);
            Assert.Equal("Foo", a.Message);
            Assert.Null(a.EventName);

            a.EventName = "Name";
            Assert.Equal("Name", a.EventName);

            // defaults
            a = new LoggerMessageAttribute();
            Assert.Equal(-1, a.EventId);
            Assert.Equal("", a.Message);
            Assert.Equal(LogLevel.None, a.Level);
        }
    }
}
