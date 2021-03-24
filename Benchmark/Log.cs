using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark
{
    static partial class Log
    {
        [LoggerMessage(380, LogLevel.Debug, @"Connection id '{connectionId}' received {type} frame for stream ID {streamId} with length {length} and flags {flags} and {other}")]
        public static partial void LogTest(ILogger logger, string connectionId, string type, string streamId, string length, string flags, string flagsd);
    }
}
