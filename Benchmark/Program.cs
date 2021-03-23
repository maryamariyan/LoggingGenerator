using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<LogBenchmark>();
        }
    }

    [MemoryDiagnoser]
    public class LogBenchmark
    {
        private ServiceProvider serviceProvider;
        private static Action<ILogger, string, string, string, string, string, string, Exception> logIterationGeneric6;
        private ILogger logger;
        private string _s1;
        private string _s2;
        private string _s3;
        private string _s4;
        private string _s5;
        private string _s6;

        [GlobalSetup]
        public void GlobalSetup()
        {
            serviceProvider = new ServiceCollection()
                .AddLogging(logBuilder =>
                {
                    logBuilder.AddConsole();
                })
                .BuildServiceProvider();
            //logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<LogBenchmark>();
            logger = NilLogger.Instance;
            _s1 = "some string";
            _s2 = "some string some string";
            _s3 = "some string some string some string";
            _s4 = "some string some string some string some string";
            _s5 = "some string some string some string some string some string";
            _s6 = "some string some string some string some string some string some string";

            logIterationGeneric6 = LoggerMessage.Define<string, string, string, string, string, string>(LogLevel.Debug,
                            eventId: 6,
                            formatString: @"Connection id '{connectionId}' received {type} frame for stream ID {streamId} with length {length} and flags {flags} and {other}");
        }

        [Benchmark]
        public void LogGeneric6UsingDefine()
        {
            logIterationGeneric6(logger, _s1, _s2, _s3, _s4, _s5, _s6, null);
        }

        [Benchmark]
        public void LogGeneric6Generated()
        {
            Log.LogTest(logger, _s1, _s2, _s3, _s4, _s5, _s6);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            serviceProvider.Dispose();
        }
    }
}
