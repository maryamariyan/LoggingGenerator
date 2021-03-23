using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSProfiler
{
    //static partial class Log
    //{
    //    [LoggerMessage(380, LogLevel.Debug, @"Connection id '{connectionId}' received {type} frame for stream ID {streamId} with length {length} and flags {flags} s {flags}")]
    //    public static partial void LogTest(ILogger logger, string connectionId, string type, string streamId, string length, string flags, string flagsd);
    //}

    static partial class Log
    {
        //[global::System.Runtime.CompilerServices.CompilerGenerated]
        private static readonly string[] _namesLogTest = new[] { "connectionId", "type", "streamId", "length", "flags", "flagsd", };

        [global::System.Runtime.CompilerServices.CompilerGenerated]
        private static readonly global::System.Func<global::Microsoft.Extensions.Logging.Internal.LogValues<string, string, string, string, string, string>, global::System.Exception?, string> _formatLogTest = (_holder, _) =>
        {
            var connectionId = _holder.Value1;
            var type = _holder.Value2;
            var streamId = _holder.Value3;
            var length = _holder.Value4;
            var flags = _holder.Value5;

            global::System.FormattableString fs = $"Connection id '{connectionId}' received {type} frame for stream ID {streamId} with length {length} and flags {flags} s {flags}";
            return global::System.FormattableString.Invariant(fs);
        };

        //[global::System.Runtime.CompilerServices.CompilerGenerated]
        public static void LogTest(Microsoft.Extensions.Logging.ILogger logger, string connectionId, string type, string streamId, string length, string flags, string flagsd)
        {
            if (logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                logger.Log(
                    global::Microsoft.Extensions.Logging.LogLevel.Debug,
                    new global::Microsoft.Extensions.Logging.EventId(380, nameof(LogTest)),
                    new global::Microsoft.Extensions.Logging.Internal.LogValues<string, string, string, string, string, string>(_formatLogTest, "Connection id '{connectionId}' received {type} frame for stream ID {streamId} with length {length} and flags {flags} s {flags}", _namesLogTest, connectionId, type, streamId, length, flags, flagsd),
                    null,
                    _formatLogTest);
            }
        }

    }
}
