// © Microsoft Corporation. All rights reserved.

namespace Microsoft.Extensions.Logging.Analyzers.Tests
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Collections.Generic;
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
                        /*1+*/logger.LogTrace(""Hello"");/*-1*/
                        /*2+*/logger.LogTrace(""Hello {arg1}"", ""One"");/*-2*/
                        /*3+*/logger.LogTrace(new Exception(), ""Hello"");/*-3*/
                        /*4+*/logger.LogTrace(new Exception(), ""Hello {arg1}"", ""One"");/*-4*/

                        /*5+*/logger.LogDebug(""Hello"");/*-5*/
                        /*6+*/logger.LogDebug(""Hello {arg1}"", ""One"");/*-6*/
                        /*7+*/logger.LogDebug(new Exception(), ""Hello"");/*-7*/
                        /*8+*/logger.LogDebug(new Exception(), ""Hello {arg1}"", ""One"");/*-8*/

                        /*9+*/logger.LogInformation(""Hello"");/*-9*/
                        /*10+*/logger.LogInformation(""Hello {arg1}"", ""One"");/*-10*/
                        /*11+*/logger.LogInformation(new Exception(), ""Hello"");/*-11*/
                        /*12+*/logger.LogInformation(new Exception(), ""Hello {arg1}"", ""One"");/*-12*/

                        /*13+*/logger.LogWarning(""Hello"");/*-13*/
                        /*14+*/logger.LogWarning(""Hello {arg1}"", ""One"");/*-14*/
                        /*15+*/logger.LogWarning(new Exception(), ""Hello"");/*-15*/
                        /*16+*/logger.LogWarning(new Exception(), ""Hello {arg1}"", ""One"");/*-16*/

                        /*17+*/logger.LogError(""Hello"");/*-17*/
                        /*18+*/logger.LogError(""Hello {arg1}"", ""One"");/*-18*/
                        /*19+*/logger.LogError(new Exception(), ""Hello"");/*-19*/
                        /*20+*/logger.LogError(new Exception(), ""Hello {arg1}"", ""One"");/*-20*/

                        /*21+*/logger.LogCritical(""Hello"");/*-21*/
                        /*22+*/logger.LogCritical(""Hello {arg1}"", ""One"");/*-22*/
                        /*23+*/logger.LogCritical(new Exception(), ""Hello"");/*-23*/
                        /*24+*/logger.LogCritical(new Exception(), ""Hello {arg1}"", ""One"");/*-24*/

                        /*25+*/logger.Log(LogLevel.Debug, ""Hello"");/*-25*/
                        /*26+*/logger.Log(LogLevel.Warning, ""Hello {arg1}"", ""One"");/*-26*/
                        /*27+*/logger.Log(LogLevel.Error, new Exception(), ""Hello"");/*-27*/
                        /*28+*/logger.Log(LogLevel.Critical, new Exception(), ""Hello {arg1}"", ""One"");/*-28*/
                    }
                }
                ";

            var (doc, disposable) = await TestFixer(programText).ConfigureAwait(false);

            for (int i = 0; i < 23; i += 4)
            {
                var level = (i / 4) switch
                {
                    0 => "Trace",
                    1 => "Debug",
                    2 => "Information",
                    3 => "Warning",
                    4 => "Error",
                    5 => "Critical",
                    _ => "Impossible",
                };

                var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(doc, MakeSpan(programText, i+1), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);
                AssertEqual(details!, 1,
                    exceptionParamIndex: -1,
                    eventIdParamIndex: -1,
                    logLevelParamIndex: -1,
                    argsIndex: 2,
                    message: "Hello",
                    level: level,
                    targetFilename: "Log.cs",
                    targetNamespace: "",
                    targetClassName: "Log",
                    targetMethodName: "Hello",
                    messageArgs: Array.Empty<string>());

                (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(doc, MakeSpan(programText, i + 2), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);
                AssertEqual(details!, 1,
                    exceptionParamIndex: -1,
                    eventIdParamIndex: -1,
                    logLevelParamIndex: -1,
                    argsIndex: 2,
                    message: "Hello {arg1}",
                    level: level,
                    targetFilename: "Log.cs",
                    targetNamespace: "",
                    targetClassName: "Log",
                    targetMethodName: "HelloArg1",
                    messageArgs: new[] { "arg1" });

                (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(doc, MakeSpan(programText, i + 3), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);
                AssertEqual(details!, 2,
                    exceptionParamIndex: 1,
                    eventIdParamIndex: -1,
                    logLevelParamIndex: -1,
                    argsIndex: 3,
                    message: "Hello",
                    level: level,
                    targetFilename: "Log.cs",
                    targetNamespace: "",
                    targetClassName: "Log",
                    targetMethodName: "Hello",
                    messageArgs: Array.Empty<string>());

                (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(doc, MakeSpan(programText, i + 4), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);
                AssertEqual(details!, 2,
                    exceptionParamIndex: 1,
                    eventIdParamIndex: -1,
                    logLevelParamIndex: -1,
                    argsIndex: 3,
                    message: "Hello {arg1}",
                    level: level,
                    targetFilename: "Log.cs",
                    targetNamespace: "",
                    targetClassName: "Log",
                    targetMethodName: "HelloArg1",
                    messageArgs: new[] { "arg1" });
            }

            {
                var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(doc, MakeSpan(programText, 25), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);
                AssertEqual(details!, 2,
                    exceptionParamIndex: -1,
                    eventIdParamIndex: -1,
                    logLevelParamIndex: 1,
                    argsIndex: 3,
                    message: "Hello",
                    level: "Debug",
                    targetFilename: "Log.cs",
                    targetNamespace: "",
                    targetClassName: "Log",
                    targetMethodName: "Hello",
                    messageArgs: Array.Empty<string>());

                (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(doc, MakeSpan(programText, 26), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);
                AssertEqual(details!, 2,
                    exceptionParamIndex: -1,
                    eventIdParamIndex: -1,
                    logLevelParamIndex: 1,
                    argsIndex: 3,
                    message: "Hello {arg1}",
                    level: "Warning",
                    targetFilename: "Log.cs",
                    targetNamespace: "",
                    targetClassName: "Log",
                    targetMethodName: "HelloArg1",
                    messageArgs: new[] { "arg1" });

                (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(doc, MakeSpan(programText, 27), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);
                AssertEqual(details!, 3,
                    exceptionParamIndex: 2,
                    eventIdParamIndex: -1,
                    logLevelParamIndex: 1,
                    argsIndex: 4,
                    message: "Hello",
                    level: "Error",
                    targetFilename: "Log.cs",
                    targetNamespace: "",
                    targetClassName: "Log",
                    targetMethodName: "Hello",
                    messageArgs: Array.Empty<string>());

                (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(doc, MakeSpan(programText, 28), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);
                AssertEqual(details!, 3,
                    exceptionParamIndex: 2,
                    eventIdParamIndex: -1,
                    logLevelParamIndex: 1,
                    argsIndex: 4,
                    message: "Hello {arg1}",
                    level: "Critical",
                    targetFilename: "Log.cs",
                    targetNamespace: "",
                    targetClassName: "Log",
                    targetMethodName: "HelloArg1",
                    messageArgs: new[] { "arg1" });
            }
            disposable.Dispose();
        }

        private static void AssertEqual(
            FixDetails fd,
            int messageParamIndex,
            int exceptionParamIndex,
            int eventIdParamIndex,
            int logLevelParamIndex,
            int argsIndex,
            string message,
            string level,
            string targetFilename,
            string? targetNamespace,
            string targetClassName,
            string targetMethodName,
            IReadOnlyList<string> messageArgs)
        {
            Assert.Equal(messageParamIndex, fd.MessageParamIndex);
            Assert.Equal(exceptionParamIndex, fd.ExceptionParamIndex);
            Assert.Equal(eventIdParamIndex, fd.EventIdParamIndex);
            Assert.Equal(logLevelParamIndex, fd.LogLevelParamIndex);
            Assert.Equal(argsIndex, fd.ArgsIndex);
            Assert.Equal(message, fd.Message);
            Assert.Equal(level, fd.Level);
            Assert.Equal(targetFilename, fd.TargetFilename);
            Assert.Equal(targetNamespace, fd.TargetNamespace);
            Assert.Equal(targetClassName, fd.TargetClassName);
            Assert.Equal(targetMethodName, fd.TargetMethodName);
            Assert.Equal(messageArgs, fd.MessageArgs);
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

        private static async Task<(Document, IDisposable)> TestFixer(string programText)
        {
            var ws = new AdhocWorkspace();
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

            return (doc, ws);
        }
    }
}
