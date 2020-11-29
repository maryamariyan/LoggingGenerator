// © Microsoft Corporation. All rights reserved.

namespace Microsoft.Extensions.Logging.Analyzers.Tests
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    public class FixerTests
    {
        class DetailsData
        {
            public string Level = string.Empty;
            public string TargetMethodName = string.Empty;
            public string Message = string.Empty;
            public int ExceptionParamIndex = -1;
            public int ArgsParamIndex = -1;
            public int MessageParamIndex = -1;
            public int LogLevelParamIndex = -1;
            public int EventIdParamIndex = -1;
            public string[] MessageArgs = Array.Empty<string>();
        }

        private readonly ITestOutputHelper _output;

        public FixerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task BasicDetails()
        {
            var invocationSourceCode = @"
                using Microsoft.Extensions.Logging;
                using System;

                class Container
                {
                    private const string Message = ""Hello"";
                    private const LogLevel Level = LogLevel.Debug; 

                    public void Test(ILogger logger)
                    {
                        /*0+*/logger.LogTrace(""Hello"");/*-0*/
                        /*1+*/logger.LogTrace(""Hello {arg1}"", ""One"");/*-1*/
                        /*2+*/logger.LogTrace(new Exception(), ""Hello"");/*-2*/
                        /*3+*/logger.LogTrace(new Exception(), ""Hello {arg1}"", ""One"");/*-3*/

                        /*4+*/logger.LogDebug(""Hello"");/*-4*/
                        /*5+*/logger.LogDebug(""Hello {arg1}"", ""One"");/*-5*/
                        /*6+*/logger.LogDebug(new Exception(), ""Hello"");/*-6*/
                        /*7+*/logger.LogDebug(new Exception(), ""Hello {arg1}"", ""One"");/*-7*/

                        /*8+*/logger.LogInformation(""Hello"");/*-8*/
                        /*9+*/logger.LogInformation(""Hello {arg1}"", ""One"");/*-9*/
                        /*10+*/logger.LogInformation(new Exception(), ""Hello"");/*-10*/
                        /*11+*/logger.LogInformation(new Exception(), ""Hello {arg1}"", ""One"");/*-11*/

                        /*12+*/logger.LogWarning(""Hello"");/*-12*/
                        /*13+*/logger.LogWarning(""Hello {arg1}"", ""One"");/*-13*/
                        /*14+*/logger.LogWarning(new Exception(), ""Hello"");/*-14*/
                        /*15+*/logger.LogWarning(new Exception(), ""Hello {arg1}"", ""One"");/*-15*/

                        /*16+*/logger.LogError(""Hello"");/*-16*/
                        /*17+*/logger.LogError(""Hello {arg1}"", ""One"");/*-17*/
                        /*18+*/logger.LogError(new Exception(), ""Hello"");/*-18*/
                        /*19+*/logger.LogError(new Exception(), ""Hello {arg1}"", ""One"");/*-19*/

                        /*20+*/logger.LogCritical(""Hello"");/*-20*/
                        /*21+*/logger.LogCritical(""Hello {arg1}"", ""One"");/*-21*/
                        /*22+*/logger.LogCritical(new Exception(), ""Hello"");/*-22*/
                        /*23+*/logger.LogCritical(new Exception(), ""Hello {arg1}"", ""One"");/*-23*/

                        /*24+*/logger.Log(LogLevel.Trace, ""Hello"");/*-24*/
                        /*25+*/logger.Log(LogLevel.Debug, ""Hello"");/*-25*/
                        /*26+*/logger.Log(LogLevel.Information, ""Hello"");/*-26*/
                        /*27+*/logger.Log(LogLevel.Warning, ""Hello {arg1}"", ""One"");/*-27*/
                        /*28+*/logger.Log(LogLevel.Error, new Exception(), ""Hello"");/*-28*/
                        /*29+*/logger.Log(LogLevel.Critical, new Exception(), ""Hello {arg1}"", ""One"");/*-29*/

                        /*30+*/logger.Log(Level, Message);/*-30*/
                    }
                }
                ";

            var nothing = Array.Empty<string>();
            var one = new[] { "arg1" };
            var data = new[]
            {
                new DetailsData { Level = "Trace",       TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Trace",       TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Trace",       TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 2, ExceptionParamIndex = 1,  ArgsParamIndex = 3 },
                new DetailsData { Level = "Trace",       TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 2, ExceptionParamIndex = 1,  ArgsParamIndex = 3 },
                new DetailsData { Level = "Debug",       TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Debug",       TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Debug",       TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 2, ExceptionParamIndex = 1,  ArgsParamIndex = 3 },
                new DetailsData { Level = "Debug",       TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 2, ExceptionParamIndex = 1,  ArgsParamIndex = 3 },
                new DetailsData { Level = "Information", TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Information", TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Information", TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 2, ExceptionParamIndex = 1,  ArgsParamIndex = 3 },
                new DetailsData { Level = "Information", TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 2, ExceptionParamIndex = 1,  ArgsParamIndex = 3 },
                new DetailsData { Level = "Warning",     TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Warning",     TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Warning",     TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 2, ExceptionParamIndex = 1,  ArgsParamIndex = 3 },
                new DetailsData { Level = "Warning",     TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 2, ExceptionParamIndex = 1,  ArgsParamIndex = 3 },
                new DetailsData { Level = "Error",       TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Error",       TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Error",       TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 2, ExceptionParamIndex = 1,  ArgsParamIndex = 3 },
                new DetailsData { Level = "Error",       TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 2, ExceptionParamIndex = 1,  ArgsParamIndex = 3 },
                new DetailsData { Level = "Critical",    TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Critical",    TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Critical",    TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 2, ExceptionParamIndex = 1,  ArgsParamIndex = 3 },
                new DetailsData { Level = "Critical",    TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 2, ExceptionParamIndex = 1,  ArgsParamIndex = 3 },

                new DetailsData { Level = "Trace",       TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 2, ExceptionParamIndex = -1, ArgsParamIndex = 3, LogLevelParamIndex = 1 },
                new DetailsData { Level = "Debug",       TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 2, ExceptionParamIndex = -1, ArgsParamIndex = 3, LogLevelParamIndex = 1 },
                new DetailsData { Level = "Information", TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 2, ExceptionParamIndex = -1, ArgsParamIndex = 3, LogLevelParamIndex = 1 },
                new DetailsData { Level = "Warning",     TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 2, ExceptionParamIndex = -1, ArgsParamIndex = 3, LogLevelParamIndex = 1 },
                new DetailsData { Level = "Error",       TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 3, ExceptionParamIndex = 2,  ArgsParamIndex = 4, LogLevelParamIndex = 1 },
                new DetailsData { Level = "Critical",    TargetMethodName = "HelloArg1", Message = "Hello {arg1}", MessageArgs = one,     MessageParamIndex = 3, ExceptionParamIndex = 2,  ArgsParamIndex = 4, LogLevelParamIndex = 1 },

                new DetailsData { Level = "Debug",       TargetMethodName = "Hello",     Message = "Hello",        MessageArgs = nothing, MessageParamIndex = 2, ExceptionParamIndex = -1, ArgsParamIndex = 3, LogLevelParamIndex = 1 },
            };

            var proj = RoslynTestUtils
                .CreateTestProject()
                    .WithLoggingBoilerplate()
                    .WithDocument("invocation.cs", invocationSourceCode);

            await proj.CommitChanges().ConfigureAwait(false);
            var invocationDoc = proj.FindDocument("invocation.cs");

            var targetClassName = "Log";
            for (int i = 0; i < data.Length; i++)
            {
                _output.WriteLine($"Iteration {i}");

                var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, MakeSpan(invocationSourceCode, i), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);

                Assert.Equal(data[i].MessageParamIndex, details!.MessageParamIndex);
                Assert.Equal(data[i].ExceptionParamIndex, details.ExceptionParamIndex);
                Assert.Equal(data[i].ArgsParamIndex, details.ArgsParamIndex);
                Assert.Equal(data[i].Message, details.Message);
                Assert.Equal(data[i].Level, details.Level);
                Assert.Equal(data[i].TargetMethodName, details.TargetMethodName);
                Assert.Equal(data[i].MessageArgs, details.MessageArgs);
                Assert.Equal(data[i].LogLevelParamIndex, details.LogLevelParamIndex);
                Assert.Equal(data[i].EventIdParamIndex, details.EventIdParamIndex);

                Assert.Equal(targetClassName, details.TargetClassName);
                Assert.Equal("Log.cs", details.TargetFilename);
                Assert.Equal("", details.TargetNamespace);
            }

            proj.Dispose();
        }

        [Fact]
        public async void UnsupportedForms()
        {
            // we just deal with the few edge cases not tackled by BasicDetails above

            var invocationSourceCode = @"
                using Microsoft.Extensions.Logging;
                using System;

                class Container
                {
                    const string Message = ""Hello"";

                    public void Test(ILogger logger)
                    {
                        /*0+*/logger.Log(LogLevel.Debug, new EventId(), ""Hello"");/*-0*/
                        /*1+*/logger.Log(LogLevel.Debug, new EventId(), new Exception(), ""Hello"");/*-1*/
                        /*2+*/logger.LogDebug(new EventId(), ""Hello"");/*-2*/
                        /*3+*/logger.LogDebug(new EventId(), new Exception(), ""Hello"");/*-3*/
                        /*4+*/logger.LogTrace("""");/*-4*/
                        /*5+*/logger.Log((LogLevel)42, ""Hello"");/*-5*/
                    }
                }
                ";

            var proj = RoslynTestUtils
                .CreateTestProject()
                    .WithLoggingBoilerplate()
                    .WithDocument("invocation.cs", invocationSourceCode);

            await proj.CommitChanges().ConfigureAwait(false);
            var invocationDoc = proj.FindDocument("invocation.cs");

            for (int i = 0; i < 3; i++)
            {
                var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, i), CancellationToken.None).ConfigureAwait(false);
                Assert.Null(invocationExpression);
                Assert.Null(details);
            }

            proj.Dispose();
        }

        [Fact]
        public async void GetFinalTargetMethodNameTest()
        {
            var targetSourceCode = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    /*0+*/static partial class Log/*-0*/
                    {
                        [LoggerMessage(0, LogLevel.Debug, ""Test2"")]
                        static partial void Test2(ILogger logger);
                    }
                }
                ";

            var invocationSourceCode = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public static void TestMethod(ILogger logger)
                        {
                            /*0+*/logger.LogInformation(""Test1"");/*-0*/
                            /*1+*/logger.LogDebug(""Test2"");/*-1*/
                        }
                    }
                }
                ";

            var proj = RoslynTestUtils
                .CreateTestProject()
                    .WithLoggingBoilerplate()
                    .WithDocument("target.cs", targetSourceCode)
                    .WithDocument("invocation.cs", invocationSourceCode);

            await proj.CommitChanges().ConfigureAwait(false);
            var targetDoc = proj.FindDocument("target.cs");
            var invocationDoc = proj.FindDocument("invocation.cs");

            var targetRoot = await targetDoc.GetSyntaxRootAsync(CancellationToken.None).ConfigureAwait(false);
            var targetClass = targetRoot!.FindNode(RoslynTestUtils.MakeSpan(targetSourceCode, 0)) as ClassDeclarationSyntax;

            for (int i = 0; i < 3; i++)
            {
                var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, 0), CancellationToken.None).ConfigureAwait(false);
                var (methodName, existing) = await LoggingFixes.GetFinalTargetMethodName(targetDoc, targetClass!, invocationDoc, invocationExpression!, details!, CancellationToken.None).ConfigureAwait(false);
                Assert.Equal("Test1", methodName);
                Assert.False(existing);

                (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, 1), CancellationToken.None).ConfigureAwait(false);
                (methodName, existing) = await LoggingFixes.GetFinalTargetMethodName(targetDoc, targetClass!, invocationDoc, invocationExpression!, details!, CancellationToken.None).ConfigureAwait(false);
                Assert.Equal("Test2", methodName);
                Assert.True(existing);
            }

            proj.Dispose();
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

        private static async Task<(Document, IDisposable)> TestFixer(string sourceCode)
        {
            var ws = new AdhocWorkspace();
            var sol = ws.AddSolution(SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create()));
            var proj = sol.AddProject("test", "test.dll", "C#")
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

            doc = doc.Project.AddDocument("test.cs", sourceCode);

            proj = doc.Project;
            sol = proj.Solution;
            Assert.True(ws.TryApplyChanges(sol));

            await AssertNoDiagnostics(proj).ConfigureAwait(false);

            return (doc, ws);
        }
    }
}
