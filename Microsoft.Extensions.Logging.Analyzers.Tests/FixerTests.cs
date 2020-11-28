// © Microsoft Corporation. All rights reserved.

namespace Microsoft.Extensions.Logging.Analyzers.Tests
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class FixerTests
    {
        [Fact]
        public void Basic()
        {
            var programText = @"
using Microsoft.Extensions.Logging;

public void Test(ILogger logger)
{{
    /*1+*/logger.LogInformation(""Hello"");/*-1*/
}}
";

            TestFixer(programText, async (doc) =>
            {
                int start = programText.IndexOf("/*1+*/", StringComparison.Ordinal) + 6;
                int end = programText.IndexOf("/*-1*/", StringComparison.Ordinal) - 1;
                var span = new TextSpan(start, end - start);
                (_, _) = await LoggingFixes.CheckIfCanFix(doc, span, CancellationToken.None).ConfigureAwait(false);
            });
        }

        private static void TestFixer(string programText, Action<Document> callback)
        {
            using var ws = new AdhocWorkspace();
            var sol = ws.AddSolution(SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create()));

            var proj = sol.AddProject("test", "test.dll", "C#");
            proj = proj.WithMetadataReferences(new[] { MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(System.Exception))!.Location) });

            var doc = proj.AddDocument("boilerplate.cs", $@"
namespace Microsoft.Extensions.Logging
{{
    public enum LogLevel
    {{
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical,
    }}

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public sealed class LoggerMessageAttribute : System.Attribute
    {{
        public LoggerMessageAttribute(int eventId, LogLevel level, string message) => (EventId, Level, Message) = (eventId, level, message);
        public int EventId {{ get; set; }}
        public string? EventName {{ get; set; }}
        public LogLevel Level {{ get; set; }}
        public string Message {{ get; set; }}
    }}

    public interface ILogger
    {{
    }}

    public static class LoggerExtensions
    {{
        public static void Log(this ILogger logger, LogLevel logLevel, Exception exception, string message, params object[] args){{}}
        public static void Log(this ILogger logger, LogLevel logLevel, EventId eventId, string message, params object[] args){{}}
        public static void Log(this ILogger logger, LogLevel logLevel, string message, params object[] args){{}}
        public static void Log(this ILogger logger, LogLevel logLevel, EventId eventId, Exception exception, string message, params object[] args){{}}
        public static void LogCritical(this ILogger logger, string message, params object[] args){{}}
        public static void LogCritical(this ILogger logger, Exception exception, string message, params object[] args){{}}
        public static void LogCritical(this ILogger logger, EventId eventId, string message, params object[] args){{}}
        public static void LogCritical(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args){{}}
        public static void LogDebug(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args){{}}
        public static void LogDebug(this ILogger logger, EventId eventId, string message, params object[] args){{}}
        public static void LogDebug(this ILogger logger, Exception exception, string message, params object[] args){{}}
        public static void LogDebug(this ILogger logger, string message, params object[] args){{}}
        public static void LogError(this ILogger logger, string message, params object[] args){{}}
        public static void LogError(this ILogger logger, Exception exception, string message, params object[] args){{}}
        public static void LogError(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args){{}}
        public static void LogError(this ILogger logger, EventId eventId, string message, params object[] args){{}}
        public static void LogInformation(this ILogger logger, EventId eventId, string message, params object[] args){{}}
        public static void LogInformation(this ILogger logger, Exception exception, string message, params object[] args){{}}
        public static void LogInformation(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args){{}}
        public static void LogInformation(this ILogger logger, string message, params object[] args){{}}
        public static void LogTrace(this ILogger logger, string message, params object[] args){{}}
        public static void LogTrace(this ILogger logger, Exception exception, string message, params object[] args){{}}
        public static void LogTrace(this ILogger logger, EventId eventId, string message, params object[] args){{}}
        public static void LogTrace(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args){{}}
        public static void LogWarning(this ILogger logger, EventId eventId, string message, params object[] args){{}}
        public static void LogWarning(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args){{}}            public static void LogWarning(this ILogger logger, string message, params object[] args){{}}
        public static void LogWarning(this ILogger logger, Exception exception, string message, params object[] args){{}}
    }}
}}
");

            doc = doc.Project.AddDocument("test.cs", programText);

            proj = doc.Project;
            sol = proj.Solution;
            Assert.True(ws.TryApplyChanges(sol));

            callback(doc);
        }
    }
}
