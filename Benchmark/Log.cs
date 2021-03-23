using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark
{
    static partial class Log
    {
        [LoggerMessage(380, LogLevel.Debug, @"Connection id '{connectionId}' received {type} frame for stream ID {streamId} with length {length} and flags {flags} and {other}")]
        public static partial void LogTest(ILogger logger, string connectionId, string type, string streamId, string length, string flags, string other);
    }

    public class NilLogger : ILogger
    {
        /// <summary>
        /// Returns the shared instance of <see cref="NilLogger"/>.
        /// </summary>
        public static NilLogger Instance { get; } = new NilLogger();

        private NilLogger()
        {
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            return default;// NullScope.Instance;
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }
    }
    public class NilLogger<T> : ILogger<T>
    {
        /// <summary>
        /// Returns an instance of <see cref="NilLogger{T}"/>.
        /// </summary>
        /// <returns>An instance of <see cref="NilLogger{T}"/>.</returns>
        public static readonly NilLogger<T> Instance = new NilLogger<T>();

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            return default;// NullScope.Instance;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        /// <inheritdoc />
        /// <remarks>
        /// This method ignores the parameters and does nothing.
        /// </remarks>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }
    }
}
