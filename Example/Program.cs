﻿// © Microsoft Corporation. All rights reserved.

using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Example
{
    class Program
    {
        public static void Main()
        {
            // normally, code receives an ILogger from dependency injection, but for the sake of this example,
            // we create one out of thin air
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddConsole().AddJsonConsole(o =>
                {
                    // This will let us see the structure going to the logger
                    o.JsonWriterOptions = new JsonWriterOptions
                    {
                        Indented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                }).AddSimpleConsole();
            });

            ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

            // The following shows use of the auto-generated logging methods, which 
            // are defined in Log.cs and generated by Microsoft.Extensions.Logging.Generators

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
            //logger.LogError("some", ); with exception source gen doesnt allow
            logger.LogWithCustomEventName();
            Log.LogWithNoTemplate(logger, "arg1", "arg2");
        }
    }
}
