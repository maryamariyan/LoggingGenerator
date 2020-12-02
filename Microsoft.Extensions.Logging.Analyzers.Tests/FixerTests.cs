// © Microsoft Corporation. All rights reserved.

namespace Microsoft.Extensions.Logging.Analyzers.Tests
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using System;
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

                        /*31+*/logger.LogCritical(""Hello {arg1:0}"", ""One"");/*-31*/
                        /*32+*/logger.LogCritical(""Hello {arg1:0"", ""One"");/*-32*/
                        /*33+*/logger.LogCritical(""Hello {{arg1}}"");/*-33*/
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

                new DetailsData { Level = "Critical",    TargetMethodName = "HelloArg10", Message = "Hello {arg1:0}", MessageArgs = one,     MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Critical",    TargetMethodName = "HelloArg10", Message = "Hello {arg1:0",  MessageArgs = nothing, MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
                new DetailsData { Level = "Critical",    TargetMethodName = "HelloArg1",  Message = "Hello {{arg1}}", MessageArgs = nothing, MessageParamIndex = 1, ExceptionParamIndex = -1, ArgsParamIndex = 2 },
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

                var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, i), CancellationToken.None).ConfigureAwait(false);
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
                    private const string NullMessage = null!;

                    public void Test(ILogger logger)
                    {
                        /*0+*/logger.Log(LogLevel.Debug, new EventId(), ""Hello"");/*-0*/
                        /*1+*/logger.Log(LogLevel.Debug, new EventId(), new Exception(), ""Hello"");/*-1*/
                        /*2+*/logger.LogDebug(new EventId(), ""Hello"");/*-2*/
                        /*3+*/logger.LogDebug(new EventId(), new Exception(), ""Hello"");/*-3*/
                        /*4+*/logger.LogTrace("""");/*-4*/
                        /*5+*/logger.Log((LogLevel)42, ""Hello"");/*-5*/
                        /*6+*/logger.LogDebug(NullMessage);/*-6*/
                        /*7+*/logger.LogDebug(null!);/*-7*/
                        /*8+*/logger.Log((LogLevel)3.1415, ""Hello"");/*-8*/
                    }
                }
                ";

            var proj = RoslynTestUtils
                .CreateTestProject()
                    .WithLoggingBoilerplate()
                    .WithDocument("invocation.cs", invocationSourceCode);

            await proj.CommitChanges().ConfigureAwait(false);
            var invocationDoc = proj.FindDocument("invocation.cs");

            for (int i = 0; i < 9; i++)
            {
                var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, i), CancellationToken.None).ConfigureAwait(false);
                Assert.Null(invocationExpression);
                Assert.Null(details);
            }

            proj.Dispose();
        }

        [Fact]
        public async void MissingPreconditions()
        {
            // make sure the code is robust to missing preconditions

            // rirst, a bogus span

            var proj = RoslynTestUtils
                .CreateTestProject()
                    .WithDocument("invocation.cs", "class FooBar {}");

            await proj.CommitChanges().ConfigureAwait(false);
            var invocationDoc = proj.FindDocument("invocation.cs");

            var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, new TextSpan(0, 10), CancellationToken.None).ConfigureAwait(false);
            Assert.Null(invocationExpression);
            Assert.Null(details);

            proj.Dispose();

            // next, try without the logging types

            var invocationSourceCode = @"
                class Container
                {
                    public static void Test()
                    {
                        /*0+*/Test2();/*-0*/
                    }

                    public static void Test2() {}
                }
                ";

            proj = RoslynTestUtils
                .CreateTestProject()
                    .WithDocument("invocation.cs", invocationSourceCode);

            await proj.CommitChanges().ConfigureAwait(false);
            invocationDoc = proj.FindDocument("invocation.cs");

            (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, 0), CancellationToken.None).ConfigureAwait(false);
            Assert.Null(invocationExpression);
            Assert.Null(details);

            proj.Dispose();

            // next, try a call to some random function

            invocationSourceCode = @"
                class Container
                {
                    public static void Test()
                    {
                        /*0+*/Test2();/*-0*/
                    }

                    public static void Test2() {}
                }
                ";

            proj = RoslynTestUtils
                .CreateTestProject()
                    .WithLoggingBoilerplate()
                    .WithDocument("invocation.cs", invocationSourceCode);

            await proj.CommitChanges().ConfigureAwait(false);
            invocationDoc = proj.FindDocument("invocation.cs");

            (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, 0), CancellationToken.None).ConfigureAwait(false);
            Assert.Null(invocationExpression);
            Assert.Null(details);

            proj.Dispose();
        }

        [Fact]
        public async void FilenameConflict()
        {
            var invocationSourceCode = @"
                using Microsoft.Extensions.Logging;

                class Container
                {
                    public void Test(ILogger logger)
                    {
                        /*0+*/logger.LogDebug(""Hello"");/*-0*/
                    }
                }
                ";

            var proj = RoslynTestUtils
                .CreateTestProject()
                    .WithLoggingBoilerplate()
                    .WithDocument("log.cs", invocationSourceCode);

            await proj.CommitChanges().ConfigureAwait(false);
            var invocationDoc = proj.FindDocument("log.cs");

            var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, 0), CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("Log2.cs", details!.TargetFilename);
            Assert.Equal("", details.TargetNamespace);
            Assert.Equal("Log", details.TargetClassName);
            Assert.Equal("Log", details.FullTargetClassName);

            proj.Dispose();
        }

        [Fact]
        public async void InNamespace()
        {
            var invocationSourceCode = @"
                using Microsoft.Extensions.Logging;

                class Container
                {
                    public void Test(ILogger logger)
                    {
                        /*0+*/logger.LogDebug(""Hello"");/*-0*/
                    }
                }
                ";

            var proj = RoslynTestUtils
                .CreateTestProject()
                    .WithLoggingBoilerplate()
                    .WithDocument("invocation.cs", invocationSourceCode)
                    .WithDefaultNamespace("Namespace");

            await proj.CommitChanges().ConfigureAwait(false);
            var invocationDoc = proj.FindDocument("invocation.cs");

            var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, 0), CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("Log.cs", details!.TargetFilename);
            Assert.Equal("Namespace", details.TargetNamespace);
            Assert.Equal("Namespace.Log", details.FullTargetClassName);

            var sol = await LoggingFixes.ApplyFix(invocationDoc, invocationExpression!, details, CancellationToken.None).ConfigureAwait(false);

            var proj2 = sol.GetProject(proj.Id)!;
            var invocationDoc2 = proj2.FindDocument("invocation.cs");
            var targetDoc = proj2.FindDocument("Log.cs");

            Assert.NotNull(invocationDoc2);
            Assert.NotNull(targetDoc);

            await proj2.AssertNoDiagnostic("CS8795").ConfigureAwait(false);

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
                        [LoggerMessage(0, LogLevel.Debug, ""TestB"")]
                        static partial void TestB(ILogger logger);

                        [LoggerMessage(1, LogLevel.Debug, ""TestCX"")]
                        static partial void TestC(ILogger logger);

                        [LoggerMessage(2, LogLevel.Debug, ""TestD {arg1}"")]
                        static partial void TestDArg1(ILogger logger, int arg1);
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
                            /*0+*/logger.LogInformation(""TestA"");/*-0*/
                            /*1+*/logger.LogDebug(""TestB"");/*-1*/
                            /*2+*/logger.LogWarning(""TestB"");/*-2*/
                            /*3+*/logger.LogWarning(""TestB"", 42);/*-3*/
                            /*4+*/logger.LogDebug(""TestC"");/*-4*/
                            /*5+*/logger.LogDebug(""TestD {arg1}"", ""Foo"");/*-5*/
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

            var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, 0), CancellationToken.None).ConfigureAwait(false);
            var (methodName, existing) = await LoggingFixes.GetFinalTargetMethodName(targetDoc, targetClass!, invocationDoc, invocationExpression!, details!, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("TestA", methodName);
            Assert.False(existing);

            (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, 1), CancellationToken.None).ConfigureAwait(false);
            (methodName, existing) = await LoggingFixes.GetFinalTargetMethodName(targetDoc, targetClass!, invocationDoc, invocationExpression!, details!, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("TestB", methodName);
            Assert.True(existing);

            (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, 2), CancellationToken.None).ConfigureAwait(false);
            (methodName, existing) = await LoggingFixes.GetFinalTargetMethodName(targetDoc, targetClass!, invocationDoc, invocationExpression!, details!, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("TestB2", methodName);
            Assert.False(existing);

            (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, 3), CancellationToken.None).ConfigureAwait(false);
            (methodName, existing) = await LoggingFixes.GetFinalTargetMethodName(targetDoc, targetClass!, invocationDoc, invocationExpression!, details!, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("TestB", methodName);
            Assert.False(existing);

            (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, 4), CancellationToken.None).ConfigureAwait(false);
            (methodName, existing) = await LoggingFixes.GetFinalTargetMethodName(targetDoc, targetClass!, invocationDoc, invocationExpression!, details!, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("TestC2", methodName);
            Assert.False(existing);

            (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, 5), CancellationToken.None).ConfigureAwait(false);
            (methodName, existing) = await LoggingFixes.GetFinalTargetMethodName(targetDoc, targetClass!, invocationDoc, invocationExpression!, details!, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("TestDArg1", methodName);
            Assert.False(existing);

            proj.Dispose();
        }

        [Fact]
        public async void ExistingTargetMethods()
        {
            var targetSourceCode = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    static partial class Log
                    {
                        [LoggerMessage(41, LogLevel.Debug, ""TestA"")]
                        static partial void TestA(ILogger logger);
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
                            /*0+*/logger.LogDebug(""TestB"");/*-0*/
                        }
                    }
                }
                ";

            var proj = RoslynTestUtils
                .CreateTestProject()
                    .WithLoggingBoilerplate()
                    .WithDefaultNamespace("Example")
                    .WithDocument("target.cs", targetSourceCode)
                    .WithDocument("invocation.cs", invocationSourceCode);

            await proj.CommitChanges().ConfigureAwait(false);
            var targetDoc = proj.FindDocument("target.cs");
            var invocationDoc = proj.FindDocument("invocation.cs");

            var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, 0), CancellationToken.None).ConfigureAwait(false);
            var sol = await LoggingFixes.ApplyFix(invocationDoc, invocationExpression!, details!, CancellationToken.None).ConfigureAwait(false);

            // TODO: check that the generated method has an event id of 42

            proj.Dispose();
        }

        [Fact]
        public async void TestApplyFix()
        {
            var invocationSourceCode = @"
                using Microsoft.Extensions.Logging;
                using System;

                namespace Example
                {
                    public class TestClass
                    {
                        public static void TestMethod(ILogger logger)
                        {
                            /*0+*/logger.LogTrace(""TestA"");/*-0*/
                            /*1+*/logger.LogTrace(new Exception(), ""TestA"");/*-1*/
                            /*2+*/logger.LogDebug(""TestB"");/*-2*/
                            /*3+*/logger.LogDebug(new Exception(), ""TestB"");/*-3*/
                            /*4+*/logger.LogInformation(""TestC"");/*-4*/
                            /*5+*/logger.LogInformation(new Exception(), ""TestC"");/*-5*/
                            /*6+*/logger.LogWarning(""TestD"");/*-6*/
                            /*7+*/logger.LogWarning(new Exception(), ""TestD"");/*-7*/
                            /*8+*/logger.LogError(""TestE"");/*-8*/
                            /*9+*/logger.LogError(new Exception(), ""TestE"");/*-9*/
                            /*10+*/logger.LogCritical(""TestF"");/*-10*/
                            /*11+*/logger.LogCritical(new Exception(), ""TestF"");/*-11*/

                            /*12+*/logger.LogTrace(""TestA"", 42);/*-12*/
                            /*13+*/logger.LogTrace(new Exception(), ""TestA"", 42);/*-13*/
                            /*14+*/logger.LogDebug(""TestB"", 42);/*-14*/
                            /*15+*/logger.LogDebug(new Exception(), ""TestB"", 42);/*-15*/
                            /*16+*/logger.LogInformation(""TestC"", 42);/*-16*/
                            /*17+*/logger.LogInformation(new Exception(), ""TestC"", 42);/*-17*/
                            /*18+*/logger.LogWarning(""TestD"", 42);/*-18*/
                            /*19+*/logger.LogWarning(new Exception(), ""TestD"", 42);/*-19*/
                            /*20+*/logger.LogError(""TestE"", 42);/*-20*/
                            /*21+*/logger.LogError(new Exception(), ""TestE"", 42);/*-21*/
                            /*22+*/logger.LogCritical(""TestF"", 42);/*-22*/
                            /*23+*/logger.LogCritical(new Exception(), ""TestF"", 42);/*-23*/

                            /*24+*/logger.Log(LogLevel.Trace, ""TestA"");/*-24*/
                            /*25+*/logger.Log(LogLevel.Trace, new Exception(), ""TestA"");/*-25*/
                            /*26+*/logger.Log(LogLevel.Debug, ""TestB"");/*-26*/
                            /*27+*/logger.Log(LogLevel.Debug, new Exception(), ""TestB"");/*-27*/
                            /*28+*/logger.Log(LogLevel.Information, ""TestC"");/*-28*/
                            /*29+*/logger.Log(LogLevel.Information, new Exception(), ""TestC"");/*-29*/
                            /*30+*/logger.Log(LogLevel.Warning, ""TestD"");/*-30*/
                            /*31+*/logger.Log(LogLevel.Warning, new Exception(), ""TestD"");/*-31*/
                            /*32+*/logger.Log(LogLevel.Error, ""TestE"");/*-32*/
                            /*33+*/logger.Log(LogLevel.Error, new Exception(), ""TestE"");/*-33*/
                            /*34+*/logger.Log(LogLevel.Critical, ""TestF"");/*-34*/
                            /*35+*/logger.Log(LogLevel.Critical, new Exception(), ""TestF"");/*-35*/
                        }
                    }
                }
                ";

            var proj = RoslynTestUtils
                .CreateTestProject()
                    .WithLoggingBoilerplate()
                    .WithDocument("invocation.cs", invocationSourceCode);

            await proj.CommitChanges().ConfigureAwait(false);

            var invocationDoc = proj.FindDocument("invocation.cs");
            for (int i = 0; i < 36; i++)
            {
                _output.WriteLine($"Iteration {i}");

                var (invocationExpression, details) = await LoggingFixes.CheckIfCanFix(invocationDoc, RoslynTestUtils.MakeSpan(invocationSourceCode, i), CancellationToken.None).ConfigureAwait(false);
                Assert.NotNull(invocationExpression);
                Assert.NotNull(details);

                var sol = await LoggingFixes.ApplyFix(invocationDoc, invocationExpression!, details!, CancellationToken.None).ConfigureAwait(false);

                var proj2 = sol.GetProject(proj.Id)!;
                var invocationDoc2 = proj2.FindDocument("invocation.cs");
                var targetDoc = proj2.FindDocument("Log.cs");

                Assert.NotNull(invocationDoc2);
                Assert.NotNull(targetDoc);

                // TODO: need to validate the generated code!

                await proj2.AssertNoDiagnostic("CS8795").ConfigureAwait(false);
            }
        }
    }
}
