// © Microsoft Corporation. All rights reserved.

using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Example
{
    class Program
    {
#if false
        public static void Test(ILogger logger)
        {
            logger.Log(LogLevel.Debug, "Hello");
            logger.LogInformation("Hello");
            logger.LogInformation("Hello");
        }
#endif

        static void Main()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddConsole().AddJsonConsole(o =>
                {
                    // This will let us see the structure going to the logger
                    o.JsonWriterOptions = new JsonWriterOptions
                    {
                        Indented = true
                    };
                });
            });

            var logger = loggerFactory.CreateLogger("LoggingExample");

            var id = Guid.NewGuid().ToString();
            Log.ConnectionStart(logger, id);

            Log.ConnectionStop(logger, id);

            Log.Http2FrameReceived(logger, id, new Http2Frame()
            {
                Flags = 2,
                PayloadLength = 100,
                StreamId = 4,
                Type = 4
            });
        }
    }
}
