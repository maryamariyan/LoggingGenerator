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
    class Program
    {
        static void Main(string[] args)
        {
#if RUNNING_CRANK
            Console.WriteLine("Application started.");
#endif

            // normally, code receives an ILogger from dependency injection, but for the sake of this example,
            // we create one out of thin air
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                //builder.AddConsole().AddJsonConsole(o =>
                //{
                //    // This will let us see the structure going to the logger
                //    o.JsonWriterOptions = new JsonWriterOptions
                //    {
                //        Indented = true,
                //        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                //    };
                //});
            });
            //var logger = loggerFactory.CreateLogger("LoggingExample");
            var logger = NullLogger.Instance;

            var _s1 = "some string";
            var _s2 = "some string some string";
            var _s3 = "some string some string some string";
            var _s4 = "some string some string some string some string";
            var _s5 = "some string some string some string some string some string";
            var _s6 = "some string some string some string some string some string some string";
            Stopwatch sw = new();
            sw.Start();
            for (int i = 0; i < 100000; i++)
            {
                Log.LogTest(logger, _s1, _s2, _s3, _s4, _s5, _s6);

            }
            sw.Stop();
#if RUNNING_CRANK

            Process process = Process.GetCurrentProcess();
            process.Refresh();

            //Console.WriteLine(process.PrivateMemorySize64 / 1024);
            //Console.WriteLine(sw.ElapsedMilliseconds);

            BenchmarksEventSource.Register("runtime/private-bytes", Operations.First, Operations.First, "Private bytes (KB)", "Private bytes (KB)", "n0");
            BenchmarksEventSource.Measure("runtime/private-bytes", process.PrivateMemorySize64 / 1024);

            BenchmarksEventSource.Register("application/elapsed-time", Operations.First, Operations.First, "Elapsed time (ms)", "Elasped time (ms)", "n0");
            BenchmarksEventSource.Measure("application/elapsed-time", sw.ElapsedMilliseconds);

            //Thread.Sleep(2000);
#endif
        }
    }
}
