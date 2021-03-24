// © Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Benchmark
{
    /// <summary>
    /// A logger which captures the last log state logged to it.
    /// </summary>
    internal class MockLogger : ILogger
    {
        private class Disposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new Disposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return Enabled;
        }

        public bool Enabled { get; set; }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (state is IReadOnlyList<KeyValuePair<string, object?>> rol)
            {
                foreach (var kvp in rol)
                {
                    // nothing
                }
            }

            _ = formatter(state, exception);
        }
    }
}
