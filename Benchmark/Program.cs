// © Microsoft Corporation. All rights reserved.

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Microsoft.Extensions.Logging;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable LA0000 // Switch to updated logging methods using the [LoggerMessage] attribute for additional performance.
#pragma warning disable R9A000 // Switch to updated logging methods using the [LoggerMessage] attribute for additional performance.

namespace Benchmark
{
    [MemoryDiagnoser]
    public class Program
    {
        private const string ConnectionId = "0x345334534678";
        private const string Type = "some string";
        private const string StreamId = "some string some string";
        private const string Length = "some string some string some string";
        private const string Flags = "some string some string some string some string";
        private const string Other = "some string some string some string some string some string";
        private const long Start = 42;
        private const long End = 123_456_789;
        private const int Options = 0x1234;

        private const int NumLoops = 100;

        private static Action<ILogger, string, string, string, string, string, string, Exception?> _loggerMessage1 = LoggerMessage.Define<string, string, string, string, string, string>(
            LogLevel.Debug,
            eventId: 380,
            formatString: @"Connection id '{connectionId}' received {type} frame for stream ID {streamId} with length {length} and flags {flags} and {other}");

        private static Action<ILogger, string, long, long, int, Exception?> _loggerMessage2 = LoggerMessage.Define<string, long, long, int>(
            LogLevel.Debug,
            eventId: 381,
            formatString: @"Connection id '{connectionId}', range [{start}..{end}], options {options}");

        private static MockLogger _logger = new MockLogger();

        public static void Main()
        {
            //var dontRequireSlnToRunBenchmarks = ManualConfig
            //    .Create(DefaultConfig.Instance)
            //    .AddJob(Job.MediumRun.WithToolchain(InProcessEmitToolchain.Instance));
            //  --inprocess --job medium (pass this to crank)

            _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run();
        }

        [Benchmark]
        public void ClassicLogging1()
        {
            _logger.Enabled = true;
            for (int i = 0; i < NumLoops; i++)
            {
                _logger.LogDebug(
                    @"Connection id '{connectionId}' received {type} frame for stream ID {streamId} with length {length} and flags {flags} and {other}",
                    ConnectionId,
                    Type,
                    StreamId,
                    Length,
                    Flags,
                    Other);
            }
        }

        [Benchmark]
        public void ClassicLogging2()
        {
            _logger.Enabled = true;
            for (int i = 0; i < NumLoops; i++)
            {
                _logger.LogDebug(@"Connection id '{connectionId}', range [{start}..{end}], options {options}", ConnectionId, Start, End, Options);
            }
        }

        [Benchmark]
        public void LoggerMessage1()
        {
            _logger.Enabled = true;
            for (int i = 0; i < NumLoops; i++)
            {
                _loggerMessage1(_logger, ConnectionId, Type, StreamId, Length, Flags, Other, null);
            }
        }

        [Benchmark]
        public void LoggerMessage2()
        {
            _logger.Enabled = true;
            for (int i = 0; i < NumLoops; i++)
            {
                _loggerMessage2(_logger, ConnectionId, Start, End, Options, null);
            }
        }

        [Benchmark]
        public void LogGen1()
        {
            _logger.Enabled = true;
            for (int i = 0; i < NumLoops; i++)
            {
                Log.LogGen1(_logger, ConnectionId, Type, StreamId, Length, Flags, Other);
            }
        }

        [Benchmark]
        public void LogGen2()
        {
            _logger.Enabled = true;
            for (int i = 0; i < NumLoops; i++)
            {
                Log.LogGen2(_logger, ConnectionId, Start, End, Options);
            }
        }

        [Benchmark]
        public void ClassicLogging1Disabled()
        {
            _logger.Enabled = false;
            for (int i = 0; i < NumLoops; i++)
            {
                _logger.LogDebug(
                    @"Connection id '{connectionId}' received {type} frame for stream ID {streamId} with length {length} and flags {flags} and {other}",
                    ConnectionId,
                    Type,
                    StreamId,
                    Length,
                    Flags,
                    Other);
            }
        }

        [Benchmark]
        public void ClassicLogging2Disabled()
        {
            _logger.Enabled = false;
            for (int i = 0; i < NumLoops; i++)
            {
                _logger.LogDebug(@"Connection id '{connectionId}', range [{start}..{end}], options {options}", ConnectionId, Start, End, Options);
            }
        }

        [Benchmark]
        public void LoggerMessage1Disabled()
        {
            _logger.Enabled = false;
            for (int i = 0; i < NumLoops; i++)
            {
                _loggerMessage1(_logger, ConnectionId, Type, StreamId, Length, Flags, Other, null);
            }
        }

        [Benchmark]
        public void LoggerMessage2Disabled()
        {
            _logger.Enabled = false;
            for (int i = 0; i < NumLoops; i++)
            {
                _loggerMessage2(_logger, ConnectionId, Start, End, Options, null);
            }
        }

        [Benchmark]
        public void LogGen1Disabled()
        {
            _logger.Enabled = false;
            for (int i = 0; i < NumLoops; i++)
            {
                Log.LogGen1(_logger, ConnectionId, Type, StreamId, Length, Flags, Other);
            }
        }

        [Benchmark]
        public void LogGen2Disabled()
        {
            _logger.Enabled = false;
            for (int i = 0; i < NumLoops; i++)
            {
                Log.LogGen2(_logger, ConnectionId, Start, End, Options);
            }
        }
    }
}
