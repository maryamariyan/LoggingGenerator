// © Microsoft Corporation. All rights reserved.

namespace Example
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Text.Json;

    class Program
    {
#if true
        public static void Test(ILogger logger)
        {
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
