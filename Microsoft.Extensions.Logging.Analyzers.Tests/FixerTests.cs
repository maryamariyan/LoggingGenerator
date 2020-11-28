// © Microsoft Corporation. All rights reserved.

namespace Microsoft.Extensions.Logging.Analyzers.Tests
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class FixerTests
    {
        [Fact]
        public async Task Basic()
        {
            var programText = @"
                using Microsoft.Extensions.Logging;
                using System;

                class Container
                {
                    public void Test(ILogger logger)
                    {
                        /*1+*/logger.LogInformation(""Hello"");/*-1*/
                        /*2+*/logger.LogInformation(""Hello {arg1}"", ""One"");/*-2*/
                        /*3+*/logger.LogInformation(new Exception(), ""Hello"");/*-3*/
                        /*4+*/logger.LogInformation(new Exception(), ""Hello {arg1}"", ""One"");/*-4*/
                    }
                }
                ";

            await TestFixer(programText, async (doc) =>
            {
                var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(doc, MakeSpan(programText, 1), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);
                Assert.True(details!.Equals(new FixDetails(1,
                    exceptionParamIndex: -1,
                    eventIdParamIndex: -1,
                    logLevelParamIndex: -1,
                    argsIndex: 2,
                    message: "Hello",
                    level: "Information",
                    targetFilename: "Log.cs",
                    targetNamespace: "",
                    targetClassName: "Log",
                    targetMethodName: "Hello",
                    messageArgs: Array.Empty<string>())));

                (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(doc, MakeSpan(programText, 2), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);
                Assert.True(details!.Equals(new FixDetails(1,
                    exceptionParamIndex: -1,
                    eventIdParamIndex: -1,
                    logLevelParamIndex: -1,
                    argsIndex: 2,
                    message: "Hello",
                    level: "Information",
                    targetFilename: "Log.cs",
                    targetNamespace: "",
                    targetClassName: "Log",
                    targetMethodName: "Hello",
                    messageArgs: new[] { "arg1" })));

                (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(doc, MakeSpan(programText, 3), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);
                Assert.True(details!.Equals(new FixDetails(1,
                    exceptionParamIndex: 1,
                    eventIdParamIndex: -1,
                    logLevelParamIndex: -1,
                    argsIndex: 3,
                    message: "Hello",
                    level: "Information",
                    targetFilename: "Log.cs",
                    targetNamespace: "",
                    targetClassName: "Log",
                    targetMethodName: "Hello",
                    messageArgs: Array.Empty<string>())));

                (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(doc, MakeSpan(programText, 3), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);
                Assert.True(details!.Equals(new FixDetails(1,
                    exceptionParamIndex: 1,
                    eventIdParamIndex: -1,
                    logLevelParamIndex: -1,
                    argsIndex: 3,
                    message: "Hello",
                    level: "Information",
                    targetFilename: "Log.cs",
                    targetNamespace: "",
                    targetClassName: "Log",
                    targetMethodName: "Hello",
                    messageArgs: new[] { "arg1" })));

            }).ConfigureAwait(false);
        }

        private static async Task AssertNoDiagnostics(Project project)
        {
            foreach (var doc in project.Documents)
            {
                var sm = await doc.GetSemanticModelAsync(CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(sm);
                Assert.Empty(sm!.GetDiagnostics());
            }
        }

        /// <summary>
        /// Looks for /*N+*/ and /*-N*/ markers in a text file and creates a TextSpan containing the enclosed text.
        /// </summary>
        private static TextSpan MakeSpan(string text, int spanNum)
        {
            int start = text.IndexOf($"/*{spanNum}+*/", StringComparison.Ordinal) + 6;
            int end = text.IndexOf($"/*-{spanNum}*/", StringComparison.Ordinal) - 1;
            return new TextSpan(start, end - start);
        }

        private static async Task TestFixer(string programText, Action<Document> callback)
        {
            using var ws = new AdhocWorkspace();
            var sol = ws.AddSolution(SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create()));

            var proj = sol.AddProject("test", "test.dll", "C#");

            proj = proj
                .WithMetadataReferences(new[] { MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(System.Exception))!.Location) })
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithNullableContextOptions(NullableContextOptions.Enable));

            var doc = proj.AddDocument("boilerplate.cs", @"
                namespace Microsoft.Extensions.Logging
                {
                    using System;

                    public enum LogLevel
                    {
                        Trace,
                        Debug,
                        Information,
                        Warning,
                        Error,
                        Critical,
                    }

                    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
                    public sealed class LoggerMessageAttribute : System.Attribute
                    {
                        public LoggerMessageAttribute(int eventId, LogLevel level, string message) => (EventId, Level, Message) = (eventId, level, message);
                        public int EventId { get; set; }
                        public string? EventName { get; set; }
                        public LogLevel Level { get; set; }
                        public string Message { get; set; }
                    }

                    public interface ILogger
                    {
                    }

                    public struct EventId
                    {
                    }

                    public static class LoggerExtensions
                    {
                        public static void Log(this ILogger logger, LogLevel logLevel, Exception exception, string message, params object[] args){}
                        public static void Log(this ILogger logger, LogLevel logLevel, EventId eventId, string message, params object[] args){}
                        public static void Log(this ILogger logger, LogLevel logLevel, string message, params object[] args){}
                        public static void Log(this ILogger logger, LogLevel logLevel, EventId eventId, Exception exception, string message, params object[] args){}
                        public static void LogCritical(this ILogger logger, string message, params object[] args){}
                        public static void LogCritical(this ILogger logger, Exception exception, string message, params object[] args){}
                        public static void LogCritical(this ILogger logger, EventId eventId, string message, params object[] args){}
                        public static void LogCritical(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args){}
                        public static void LogDebug(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args){}
                        public static void LogDebug(this ILogger logger, EventId eventId, string message, params object[] args){}
                        public static void LogDebug(this ILogger logger, Exception exception, string message, params object[] args){}
                        public static void LogDebug(this ILogger logger, string message, params object[] args){}
                        public static void LogError(this ILogger logger, string message, params object[] args){}
                        public static void LogError(this ILogger logger, Exception exception, string message, params object[] args){}
                        public static void LogError(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args){}
                        public static void LogError(this ILogger logger, EventId eventId, string message, params object[] args){}
                        public static void LogInformation(this ILogger logger, EventId eventId, string message, params object[] args){}
                        public static void LogInformation(this ILogger logger, Exception exception, string message, params object[] args){}
                        public static void LogInformation(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args){}
                        public static void LogInformation(this ILogger logger, string message, params object[] args){}
                        public static void LogTrace(this ILogger logger, string message, params object[] args){}
                        public static void LogTrace(this ILogger logger, Exception exception, string message, params object[] args){}
                        public static void LogTrace(this ILogger logger, EventId eventId, string message, params object[] args){}
                        public static void LogTrace(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args){}
                        public static void LogWarning(this ILogger logger, EventId eventId, string message, params object[] args){}
                        public static void LogWarning(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args){}
                        public static void LogWarning(this ILogger logger, string message, params object[] args){}
                        public static void LogWarning(this ILogger logger, Exception exception, string message, params object[] args){}
                    }
                }
                ");

            doc = doc.Project.AddDocument("test.cs", programText);

            proj = doc.Project;
            sol = proj.Solution;
            Assert.True(ws.TryApplyChanges(sol));

            await AssertNoDiagnostics(proj).ConfigureAwait(false);

            callback(doc);
        }
    }
}
