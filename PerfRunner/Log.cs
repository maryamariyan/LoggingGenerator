using System;
using System.Diagnostics;
using System.Threading;
#if RUNNING_CRANK
using Microsoft.Crank.EventSources;
#endif
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;

namespace PerfRunner
{
    static partial class Log
    {
        [LoggerMessage(380, LogLevel.Debug, @"Connection id '{connectionId}' received {type} frame for stream ID {streamId} with length {length} and flags {flags} and {other}")]
        public static partial void LogTest(ILogger logger, string connectionId, string type, string streamId, string length, string flags, string other);
    }

}
