﻿// © Microsoft Corporation. All rights reserved.

using Microsoft.Extensions.Logging;

namespace Example
{
// Bug in this analyzer, it's getting confused
#pragma warning disable CA1801 // Review unused parameters

    // This shows how a developer can declare their logging messages, which will be turned into
    // full-fledged logging methods by the code generation logic.
    //
    // Any method annotated with [LoggerMessage] triggers the code generator. This means it is
    // possible to have utility methods or other completely unrelated content in classes
    // that have logger messages, if a developer choses to.
    static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Critical, Message = "Could not open socket to `{hostName}`")]
        public static partial void CouldNotOpenSocket(ILogger logger, string hostName);

        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = @"Connection id '{connectionId}' started.")]
        public static partial void ConnectionStart(ILogger logger, string connectionId);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = @"Connection id '{connectionId}' stopped.")]
        public static partial void ConnectionStop(ILogger logger, string connectionId);

        [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = @"Connection id '{connectionId}' paused.")]
        public static partial void ConnectionPause(ILogger logger, string connectionId);

        [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = @"Connection id '{connectionId}' resume.")]
        public static partial void ConnectionResume(ILogger logger, string connectionId);

        [LoggerMessage(EventId = 9, Level = LogLevel.Debug, Message = @"Connection id '{connectionId}' completed keep alive response.")]
        public static partial void ConnectionKeepAlive(ILogger logger, string connectionId);

        [LoggerMessage(EventId = 38, Level = LogLevel.Debug, Message = @"Connection id '{connectionId}' received {type} frame for stream ID {streamId} with length {length} and flags {flags}")]
        public static partial void Http2FrameReceived(ILogger logger, string connectionId, byte type, int streamId, int length, byte flags);

        // Not a logger message
        public static void Http2FrameReceived(ILogger logger, string connectionId, Http2Frame http2Frame)
        {
            Http2FrameReceived(logger, connectionId, http2Frame.Type, http2Frame.StreamId, http2Frame.PayloadLength, http2Frame.Flags);
        }
    }

    public class Http2Frame
    {
        public int PayloadLength { get; set; }
        public byte Type { get; set; }
        public byte Flags { get; set; }
        public int StreamId { get; set; }
    }
}
