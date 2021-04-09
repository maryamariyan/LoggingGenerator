// © Microsoft Corporation. All rights reserved.
#define LOGGER_MESSAGE_DEFINE

using System;

#pragma warning disable CA1801 // Review unused parameters

namespace Microsoft.Extensions.Logging.Generators.Test.TestClasses
{
    internal static partial class ArgTestExtensions
    {
        [LoggerMessage(0, LogLevel.Error, "M1")]
        public static partial void Method1(ILogger logger);

        [LoggerMessage(1, LogLevel.Error, "M2 {p1}")]
        public static partial void Method2(ILogger logger, string p1);

        [LoggerMessage(2, LogLevel.Error, "M3 {p1} {p2}")]
        public static partial void Method3(ILogger logger, string p1, int p2);

        [LoggerMessage(3, LogLevel.Error, "M4")]
        public static partial void Method4(ILogger logger, InvalidOperationException p1);

        [LoggerMessage(4, LogLevel.Error, "M5 {p2}")]
        public static partial void Method5(ILogger logger, System.InvalidOperationException p1, System.InvalidOperationException p2);

        [LoggerMessage(5, LogLevel.Error, "M6 {p2}")]
        public static partial void Method6(ILogger logger, System.InvalidOperationException p1, int p2);

        [LoggerMessage(6, LogLevel.Error, "M7 {p1}")]
        public static partial void Method7(ILogger logger, int p1, System.InvalidOperationException p2);

#if !LOGGER_MESSAGE_DEFINE
        [LoggerMessage(1000, LogLevel.Error, "M1000{p1}{p2}{p3}{p4}{p5}{p6}{p7}{p8}{p9}{p10}{p11}{p12}{p13}{p14}{p15}{p16}{p17}{p18}{p19}")]
#pragma warning disable S1000 // Methods should not have too many parameters
        public static partial void Method1000(ILogger logger,
        int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9, int p10,
        int p11, int p12, int p13, int p14, int p15, int p16, int p17, int p18, int p19);
#pragma warning restore S1000 // Methods should not have too many parameters

#pragma warning disable S107 // Methods should not have too many parameters
        [LoggerMessage(7, LogLevel.Error, "M8{p1}{p2}{p3}{p4}{p5}{p6}{p7}")]
        public static partial void Method8(ILogger logger, int p1, int p2, int p3, int p4, int p5, int p6, int p7);

        [LoggerMessage(8, LogLevel.Error, "M9 {p1} {p2} {p3} {p4} {p5} {p6} {p7}")]
        public static partial void Method9(ILogger logger, int p1, int p2, int p3, int p4, int p5, int p6, int p7);
#endif
#pragma warning restore S107 // Methods should not have too many parameters

        [LoggerMessage(9, LogLevel.Error, "M10{p1}")]
        public static partial void Method10(ILogger logger, int p1);
    }
}
