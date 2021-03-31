// © Microsoft Corporation. All rights reserved.

using Microsoft.Extensions.Logging;

#pragma warning disable CA1801 // Review unused parameters
#pragma warning disable S109

namespace Benchmark
{
    internal static partial class Log
    {
        [LoggerMessage(380, LogLevel.Debug, @"Connection id '{connectionId}' received {type} frame for stream ID {streamId} with length {length} and flags {flags} and {other}")]
        public static partial void LogGen1(ILogger logger, string connectionId, string type, string streamId, string length, string flags, string other);

        [LoggerMessage(381, LogLevel.Debug, @"Connection id '{connectionId}', range [{start}..{end}], options {options}")]
        public static partial void LogGen2(ILogger logger, string connectionId, long start, long end, int options);
    }
}
