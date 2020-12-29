// © Microsoft Corporation. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Microsoft.Extensions.Logging.Analyzers.Test
{
    public class UsingLegacyLoggingMethodTests
    {
        [Fact]
        public async Task Basic()
        {
            var originalTarget = @"
                static partial class Log
                {
                }
                ";

            var originalSource = @"
                using Microsoft.Extensions.Logging;
                using System;

                namespace Example
                {
                    public class TestClass
                    {
                        private const string Message = ""Hello"";
                        private const LogLevel Level = LogLevel.Debug;
                        private const string NullMessage = null!;

                        public void Test(ILogger logger)
                        {
                            logger.LogTrace(""Hello"");
                            logger.LogTrace(""Hello {arg1}"", ""One"");
                            logger.LogTrace(new Exception(), ""Hello"");
                            logger.LogTrace(new Exception(), ""Hello {arg1}"", ""One"");

                            logger.LogDebug(""Hello"");
                            logger.LogDebug(""Hello {arg1}"", ""One"");
                            logger.LogDebug(new Exception(), ""Hello"");
                            logger.LogDebug(new Exception(), ""Hello {arg1}"", ""One"");

                            logger.LogInformation(""Hello"");
                            logger.LogInformation(""Hello {arg1}"", ""One"");
                            logger.LogInformation(new Exception(), ""Hello"");
                            logger.LogInformation(new Exception(), ""Hello {arg1}"", ""One"");

                            logger.LogWarning(""Hello"");
                            logger.LogWarning(""Hello {arg1}"", ""One"");
                            logger.LogWarning(new Exception(), ""Hello"");
                            logger.LogWarning(new Exception(), ""Hello {arg1}"", ""One"");

                            logger.LogError(""Hello"");
                            logger.LogError(""Hello {arg1}"", ""One"");
                            logger.LogError(new Exception(), ""Hello"");
                            logger.LogError(new Exception(), ""Hello {arg1}"", ""One"");

                            logger.LogCritical(""Hello"");
                            logger.LogCritical(""Hello {arg1}"", ""One"");
                            logger.LogCritical(new Exception(), ""Hello"");
                            logger.LogCritical(new Exception(), ""Hello {arg1}"", ""One"");

                            logger.Log(LogLevel.Trace, ""Hello"");
                            logger.Log(LogLevel.Debug, ""Hello"");
                            logger.Log(LogLevel.Information, ""Hello"");
                            logger.Log(LogLevel.Warning, ""Hello {arg1}"", ""One"");
                            logger.Log(LogLevel.Error, new Exception(), ""Hello"");
                            logger.Log(LogLevel.Critical, new Exception(), ""Hello {arg1}"", ""One"");

                            logger.Log(Level, Message);

                            logger.LogCritical(""Hello {arg1:0}"", ""One"");
                            logger.LogCritical(""Hello {arg1:0"", ""One"");
                            logger.LogCritical(""Hello {{arg1}}"");

                            logger.Log(LogLevel.Debug, new EventId(), ""Hello"");
                            logger.Log(LogLevel.Debug, new EventId(), new Exception(), ""Hello"");
                            logger.LogDebug(new EventId(), ""Hello"");
                            logger.LogDebug(new EventId(), new Exception(), ""Hello"");
                            logger.LogTrace("""");
                            logger.Log((LogLevel)42, ""Hello"");
                            logger.LogDebug(NullMessage);
                            logger.LogDebug(null!);
                            logger.Log((LogLevel)3.1415, ""Hello"");
                        }
                    }
                }
                ";

            var expectedSource = @"
                using Microsoft.Extensions.Logging;
                using System;

                namespace Example
                {
                    public class TestClass
                    {
                        private const string Message = ""Hello"";
                        private const LogLevel Level = LogLevel.Debug;
                        private const string NullMessage = null!;

                        public void Test(ILogger logger)
                        {
            Log.Hello(logger);
            Log.HelloArg1(logger, ""One"");
            Log.Hello(logger, new Exception());
            Log.HelloArg1(logger, new Exception(), ""One"");

            Log.Hello2(
                            logger);
            Log.HelloArg12(logger, ""One"");
            Log.Hello2(logger, new Exception());
            Log.HelloArg12(logger, new Exception(), ""One"");

            Log.Hello2(
                            logger);
            Log.HelloArg12(logger, ""One"");
            Log.Hello2(logger, new Exception());
            Log.HelloArg12(logger, new Exception(), ""One"");

            Log.Hello2(
                            logger);
            Log.HelloArg12(logger, ""One"");
            Log.Hello2(logger, new Exception());
            Log.HelloArg12(logger, new Exception(), ""One"");

            Log.Hello2(
                            logger);
            Log.HelloArg12(logger, ""One"");
            Log.Hello2(logger, new Exception());
            Log.HelloArg12(logger, new Exception(), ""One"");

            Log.Hello2(
                            logger);
            Log.HelloArg12(logger, ""One"");
            Log.Hello2(logger, new Exception());
            Log.HelloArg12(logger, new Exception(), ""One"");

            Log.Hello(
                            logger);
            Log.Hello(logger);
            Log.Hello(logger);
            Log.HelloArg1(logger, ""One"");
            Log.Hello(logger, new Exception());
            Log.HelloArg1(logger, new Exception(), ""One"");

            Log.Hello(
                            logger);

            Log.HelloArg10(
                            logger, ""One"");
            Log.HelloArg102(logger, ""One"");
            Log.HelloArg1(logger);

                            logger.Log(LogLevel.Debug, new EventId(), ""Hello"");
                            logger.Log(LogLevel.Debug, new EventId(), new Exception(), ""Hello"");
                            logger.LogDebug(new EventId(), ""Hello"");
                            logger.LogDebug(new EventId(), new Exception(), ""Hello"");
                            logger.LogTrace("""");
                            logger.Log((LogLevel)42, ""Hello"");
                            logger.LogDebug(NullMessage);
                            logger.LogDebug(null!);
                            logger.Log((LogLevel)3.1415, ""Hello"");
                        }
                    }
                }
                ";

            var expectedTarget = @"
                static partial class Log
                {

    /// <summary>
    /// Logs `Hello` at `Trace` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(0, Microsoft.Extensions.Logging.LogLevel.Trace, ""Hello"")]
    internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger);

    /// <summary>
    /// Logs `Hello {arg1}` at `Trace` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(1, Microsoft.Extensions.Logging.LogLevel.Trace, ""Hello {arg1}"")]
    internal static partial void HelloArg1(Microsoft.Extensions.Logging.ILogger logger, string arg1);

    /// <summary>
    /// Logs `Hello` at `Trace` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(2, Microsoft.Extensions.Logging.LogLevel.Trace, ""Hello"")]
    internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger, System.Exception exception);

    /// <summary>
    /// Logs `Hello {arg1}` at `Trace` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(3, Microsoft.Extensions.Logging.LogLevel.Trace, ""Hello {arg1}"")]
    internal static partial void HelloArg1(Microsoft.Extensions.Logging.ILogger logger, System.Exception exception, string arg1);

    /// <summary>
    /// Logs `Hello` at `Debug` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(4, Microsoft.Extensions.Logging.LogLevel.Debug, ""Hello"")]
    internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger);

    /// <summary>
    /// Logs `Hello {arg1}` at `Debug` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(5, Microsoft.Extensions.Logging.LogLevel.Debug, ""Hello {arg1}"")]
    internal static partial void HelloArg1(Microsoft.Extensions.Logging.ILogger logger, string arg1);

    /// <summary>
    /// Logs `Hello` at `Debug` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(6, Microsoft.Extensions.Logging.LogLevel.Debug, ""Hello"")]
    internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger, System.Exception exception);

    /// <summary>
    /// Logs `Hello {arg1}` at `Debug` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(7, Microsoft.Extensions.Logging.LogLevel.Debug, ""Hello {arg1}"")]
    internal static partial void HelloArg1(Microsoft.Extensions.Logging.ILogger logger, System.Exception exception, string arg1);

    /// <summary>
    /// Logs `Hello` at `Information` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(8, Microsoft.Extensions.Logging.LogLevel.Information, ""Hello"")]
    internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger);

    /// <summary>
    /// Logs `Hello {arg1}` at `Information` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(9, Microsoft.Extensions.Logging.LogLevel.Information, ""Hello {arg1}"")]
    internal static partial void HelloArg1(Microsoft.Extensions.Logging.ILogger logger, string arg1);

    /// <summary>
    /// Logs `Hello` at `Information` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(10, Microsoft.Extensions.Logging.LogLevel.Information, ""Hello"")]
    internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger, System.Exception exception);

    /// <summary>
    /// Logs `Hello {arg1}` at `Information` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(11, Microsoft.Extensions.Logging.LogLevel.Information, ""Hello {arg1}"")]
    internal static partial void HelloArg1(Microsoft.Extensions.Logging.ILogger logger, System.Exception exception, string arg1);

    /// <summary>
    /// Logs `Hello` at `Warning` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(12, Microsoft.Extensions.Logging.LogLevel.Warning, ""Hello"")]
    internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger);

    /// <summary>
    /// Logs `Hello {arg1}` at `Warning` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(13, Microsoft.Extensions.Logging.LogLevel.Warning, ""Hello {arg1}"")]
    internal static partial void HelloArg1(Microsoft.Extensions.Logging.ILogger logger, string arg1);

    /// <summary>
    /// Logs `Hello` at `Warning` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(14, Microsoft.Extensions.Logging.LogLevel.Warning, ""Hello"")]
    internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger, System.Exception exception);

    /// <summary>
    /// Logs `Hello {arg1}` at `Warning` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(15, Microsoft.Extensions.Logging.LogLevel.Warning, ""Hello {arg1}"")]
    internal static partial void HelloArg1(Microsoft.Extensions.Logging.ILogger logger, System.Exception exception, string arg1);

    /// <summary>
    /// Logs `Hello` at `Error` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(16, Microsoft.Extensions.Logging.LogLevel.Error, ""Hello"")]
    internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger);

    /// <summary>
    /// Logs `Hello {arg1}` at `Error` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(17, Microsoft.Extensions.Logging.LogLevel.Error, ""Hello {arg1}"")]
    internal static partial void HelloArg1(Microsoft.Extensions.Logging.ILogger logger, string arg1);

    /// <summary>
    /// Logs `Hello` at `Error` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(18, Microsoft.Extensions.Logging.LogLevel.Error, ""Hello"")]
    internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger, System.Exception exception);

    /// <summary>
    /// Logs `Hello {arg1}` at `Error` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(19, Microsoft.Extensions.Logging.LogLevel.Error, ""Hello {arg1}"")]
    internal static partial void HelloArg1(Microsoft.Extensions.Logging.ILogger logger, System.Exception exception, string arg1);

    /// <summary>
    /// Logs `Hello` at `Critical` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(20, Microsoft.Extensions.Logging.LogLevel.Critical, ""Hello"")]
    internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger);

    /// <summary>
    /// Logs `Hello {arg1}` at `Critical` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(21, Microsoft.Extensions.Logging.LogLevel.Critical, ""Hello {arg1}"")]
    internal static partial void HelloArg1(Microsoft.Extensions.Logging.ILogger logger, string arg1);

    /// <summary>
    /// Logs `Hello` at `Critical` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(22, Microsoft.Extensions.Logging.LogLevel.Critical, ""Hello"")]
    internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger, System.Exception exception);

    /// <summary>
    /// Logs `Hello {arg1}` at `Critical` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(23, Microsoft.Extensions.Logging.LogLevel.Critical, ""Hello {arg1}"")]
    internal static partial void HelloArg1(Microsoft.Extensions.Logging.ILogger logger, System.Exception exception, string arg1);

    /// <summary>
    /// Logs `Hello {arg1:0}` at `Critical` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(24, Microsoft.Extensions.Logging.LogLevel.Critical, ""Hello {arg1:0}"")]
    internal static partial void HelloArg10(Microsoft.Extensions.Logging.ILogger logger, string arg1);

    /// <summary>
    /// Logs `Hello {arg1:0` at `Critical` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(25, Microsoft.Extensions.Logging.LogLevel.Critical, ""Hello {arg1:0"")]
    internal static partial void HelloArg10(Microsoft.Extensions.Logging.ILogger logger, string arg0);

    /// <summary>
    /// Logs `Hello {{arg1}}` at `Critical` level.
    /// </summary>
    [Microsoft.Extensions.Logging.LoggerMessage(26, Microsoft.Extensions.Logging.LogLevel.Critical, ""Hello {{arg1}}"")]
    internal static partial void HelloArg1(Microsoft.Extensions.Logging.ILogger logger);
}
                ";
 
            var (actualSource, actualTarget) = await FixerRunner.ApplyAllFixes(
                new UsingLegacyLoggingMethodAnalyzer(), new UsingLegacyLoggingMethodFixer(),
                originalSource, originalTarget).ConfigureAwait(false);

            Assert.Equal(expectedSource.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualSource);
            Assert.Equal(expectedTarget.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualTarget);
        }

        [Fact]
        public async Task TargetClassDoesntExist()
        {
            var originalTarget = @"
                ";

            var originalSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
                            logger.LogTrace(""Hello"");
                        }
                    }
                }
                ";

            var expectedSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
            Log.Hello(logger);
                        }
                    }
                }
                ";

            var expectedTarget = @"
#pragma warning disable CS8019
using Microsoft.Extensions.Logging;
using System;
#pragma warning restore CS8019

static partial class Log
{

    /// <summary>
    /// Logs `Hello` at `Trace` level.
    /// </summary>
    [LoggerMessage(0, LogLevel.Trace, ""Hello"")]
    internal static partial void Hello(ILogger logger);
}
";

            var (actualSource, actualTarget) = await FixerRunner.ApplyAllFixes(
                new UsingLegacyLoggingMethodAnalyzer(), new UsingLegacyLoggingMethodFixer(),
                originalSource, originalTarget, "Log.cs").ConfigureAwait(false);

            Assert.Equal(expectedSource.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualSource);
            Assert.Equal(expectedTarget.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualTarget);
        }

        [Fact]
        public async Task TargetClassInNamespace()
        {
            var originalTarget = @"
                namespace Example
                {
                    public static partial class Log
                    {
                    }
                }
                ";

            var originalSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
                            logger.LogTrace(""Hello"");
                        }
                    }
                }
                ";

            var expectedSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
            Log.Hello(logger);
                        }
                    }
                }
                ";
            var expectedTarget = @"
                namespace Example
                {
                    public static partial class Log
                    {

        /// <summary>
        /// Logs `Hello` at `Trace` level.
        /// </summary>
        [Microsoft.Extensions.Logging.LoggerMessage(0, Microsoft.Extensions.Logging.LogLevel.Trace, ""Hello"")]
        internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger);
    }
                }
                ";

            var (actualSource, actualTarget) = await FixerRunner.ApplyAllFixes(
                new UsingLegacyLoggingMethodAnalyzer(), new UsingLegacyLoggingMethodFixer(),
                originalSource, originalTarget, null, "Example").ConfigureAwait(false);

            Assert.Equal(expectedSource.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualSource);
            Assert.Equal(expectedTarget.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualTarget);
        }

        [Fact]
        public async Task TargetClassDoesntExistWithNamespace()
        {
            var originalTarget = @"
                ";

            var originalSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
                            logger.LogTrace(""Hello"");
                        }
                    }
                }
                ";

            var expectedSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
            Log.Hello(logger);
                        }
                    }
                }
                ";
            var expectedTarget = @"
namespace Example
{
#pragma warning disable CS8019
    using Microsoft.Extensions.Logging;
    using System;
#pragma warning restore CS8019

    static partial class Log
    {

        /// <summary>
        /// Logs `Hello` at `Trace` level.
        /// </summary>
        [LoggerMessage(0, LogLevel.Trace, ""Hello"")]
        internal static partial void Hello(ILogger logger);
    }
}
";

            var (actualSource, actualTarget) = await FixerRunner.ApplyAllFixes(
                new UsingLegacyLoggingMethodAnalyzer(), new UsingLegacyLoggingMethodFixer(),
                originalSource, originalTarget, "Log.cs", "Example").ConfigureAwait(false);

            Assert.Equal(expectedSource.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualSource);
            Assert.Equal(expectedTarget.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualTarget);
        }

        [Fact]
        public async Task TargetClassExistWithDeepNamespace()
        {
            var originalTarget = @"
                namespace Example
                {
                    namespace Example2
                    {
                        static partial class Log
                        {
                        }
                    }
                }
                ";

            var originalSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
                            logger.LogTrace(""Hello"");
                        }
                    }
                }
                ";

            var expectedSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
            Example2.Log.Hello(logger);
                        }
                    }
                }
                ";

            var expectedTarget = @"
                namespace Example
                {
                    namespace Example2
                    {
                        static partial class Log
                        {

            /// <summary>
            /// Logs `Hello` at `Trace` level.
            /// </summary>
            [Microsoft.Extensions.Logging.LoggerMessage(0, Microsoft.Extensions.Logging.LogLevel.Trace, ""Hello"")]
            internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger);
        }
                    }
                }
                ";

            var (actualSource, actualTarget) = await FixerRunner.ApplyAllFixes(
                new UsingLegacyLoggingMethodAnalyzer(), new UsingLegacyLoggingMethodFixer(),
                originalSource, originalTarget, null, "Example.Example2").ConfigureAwait(false);

            Assert.Equal(expectedSource.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualSource);
            Assert.Equal(expectedTarget.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualTarget);
        }

        [Fact]
        public async Task TargetClassDoesntExistInNestedType()
        {
            var originalTarget = @"
                namespace Example
                {
                    class Container
                    {
                        static partial class Log
                        {
                        }
                    }
                }
                ";

            var originalSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
                            logger.LogTrace(""Hello"");
                        }
                    }
                }
                ";

            var expectedSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
            Example2.Log.Hello(logger);
                        }
                    }
                }
                ";

            var expectedTarget = @"
namespace Example.Example2
{
#pragma warning disable CS8019
    using Microsoft.Extensions.Logging;
    using System;
#pragma warning restore CS8019

    static partial class Log
    {

        /// <summary>
        /// Logs `Hello` at `Trace` level.
        /// </summary>
        [LoggerMessage(0, LogLevel.Trace, ""Hello"")]
        internal static partial void Hello(ILogger logger);
    }
}
";

            var (actualSource, actualTarget) = await FixerRunner.ApplyAllFixes(
                new UsingLegacyLoggingMethodAnalyzer(), new UsingLegacyLoggingMethodFixer(),
                originalSource, originalTarget, "Log.cs", "Example.Example2").ConfigureAwait(false);

            Assert.Equal(expectedSource.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualSource);
            Assert.Equal(expectedTarget.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualTarget);
        }

        [Fact]
        public async Task DuplicateFilename()
        {
            var originalTarget = @"
                namespace Example
                {
                }
                ";

            var originalSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
                            logger.LogTrace(""Hello"");
                        }
                    }
                }
                ";

            var expectedSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
            Log.Hello(logger);
                        }
                    }
                }
                ";

            var expectedTarget = @"
namespace Example
{
#pragma warning disable CS8019
    using Microsoft.Extensions.Logging;
    using System;
#pragma warning restore CS8019

    static partial class Log
    {

        /// <summary>
        /// Logs `Hello` at `Trace` level.
        /// </summary>
        [LoggerMessage(0, LogLevel.Trace, ""Hello"")]
        internal static partial void Hello(ILogger logger);
    }
}
";

            var (actualSource, actualTarget) = await FixerRunner.ApplyAllFixes(
                new UsingLegacyLoggingMethodAnalyzer(), new UsingLegacyLoggingMethodFixer(),
                originalSource, originalTarget, "Log2.cs", "Example", "Log.cs").ConfigureAwait(false);

            Assert.Equal(expectedSource.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualSource);
            Assert.Equal(expectedTarget.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualTarget);
        }

        [Fact]
        public async Task MissingMetadata()
        {
            var originalTarget = @"
                namespace Example
                {
                    public static partial class Log
                    {
                    }
                }
                ";

            var originalSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
                            logger.LogTrace(""Hello"");
                        }
                    }
                }
                ";

            var expectedSource = @"
                using Microsoft.Extensions.Logging;

                namespace Example
                {
                    public class TestClass
                    {
                        public void Test(ILogger logger)
                        {
            Log.Hello(logger);
                        }
                    }
                }
                ";

            var expectedTarget = @"
                namespace Example
                {
                    public static partial class Log
                    {

        /// <summary>
        /// Logs `Hello` at `Trace` level.
        /// </summary>
        [Microsoft.Extensions.Logging.LoggerMessage(1, Microsoft.Extensions.Logging.LogLevel.Trace, ""Hello"")]
        internal static partial void Hello(Microsoft.Extensions.Logging.ILogger logger);
    }
                }
                ";

            var (actualSource, actualTarget) = await FixerRunner.ApplyAllFixes(
                new UsingLegacyLoggingMethodAnalyzer(), new UsingLegacyLoggingMethodFixer
                {
                    _getTypeByMetadataName3 = (c, n) => null,
                },
                originalSource, originalTarget, null, "Example").ConfigureAwait(false);

            Assert.Equal(expectedSource.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualSource);
            Assert.Equal(expectedTarget.Replace("\r\n", "\n", System.StringComparison.InvariantCulture), actualTarget);
        }

        [Fact]
        public void UtilityMethods()
        {
            var f = new UsingLegacyLoggingMethodFixer();
            Assert.Single(f.FixableDiagnosticIds);
            Assert.Equal(WellKnownFixAllProviders.BatchFixer, f.GetFixAllProvider());
        }

        [Fact]
        public async Task FailureModes()
        {
            var targetSourceCode = @"
                using Microsoft.Extensions.Logging;
                using System;
                using System.Runtime.CompilerServices;

                namespace Example
                {
                    namespace Example2
                    {
                        /*0+*/static partial class Log/*-0*/
                        {
                            [Microsoft.Extensions.Logging.LoggerMessage(0, LogLevel.Trace, ""Hello"")]
                            internal static void Hello(ILogger logger) {}

                            [Obsolete]
                            [MethodImpl(MethodImplOptions.AggressiveInlining)]
                            internal static void World() {}
                        }
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
                            /*0+*/logger.LogTrace(""TestA"");/*-0*/
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
            var targetRoot = await targetDoc.GetSyntaxRootAsync(CancellationToken.None).ConfigureAwait(false);
            var targetClass = targetRoot!.FindNode(RoslynTestUtils.MakeSpan(targetSourceCode, 0)) as ClassDeclarationSyntax;
            var invocationDoc = proj.FindDocument("invocation.cs");

            // make sure this works normally
            var f = new UsingLegacyLoggingMethodFixer();
            var (invocationExpression, details) = await f.CheckIfCanFixAsync(
                invocationDoc,
                RoslynTestUtils.MakeSpan(invocationSourceCode, 0),
                CancellationToken.None).ConfigureAwait(false);

            Assert.NotNull(invocationExpression);
            Assert.NotNull(details);

            var (methodName, existing) = await f.GetFinalTargetMethodNameAsync(
                targetDoc,
                targetClass!,
                invocationDoc,
                invocationExpression!,
                details!,
                CancellationToken.None).ConfigureAwait(false);

            Assert.Equal("TestA", methodName);
            Assert.False(existing);

            f = new UsingLegacyLoggingMethodFixer
            {
                _getSyntaxRootAsync = (d, t) =>
                {
                    return Task.FromResult<SyntaxNode?>(null);
                }
            };

            (invocationExpression, details) = await f.CheckIfCanFixAsync(
                invocationDoc,
                RoslynTestUtils.MakeSpan(invocationSourceCode, 0),
                CancellationToken.None).ConfigureAwait(false);

            Assert.Null(invocationExpression);
            Assert.Null(details);

            f = new UsingLegacyLoggingMethodFixer
            {
                _getSemanticModelAsync = (d, t) =>
                {
                    return Task.FromResult<SemanticModel?>(null);
                }
            };

            (invocationExpression, details) = await f.CheckIfCanFixAsync(
                invocationDoc,
                RoslynTestUtils.MakeSpan(invocationSourceCode, 0),
                CancellationToken.None).ConfigureAwait(false);

            Assert.Null(invocationExpression);
            Assert.Null(details);

            f = new UsingLegacyLoggingMethodFixer
            {
                _getOperation = (sm, sn, t) =>
                {
                    return null;
                }
            };

            (invocationExpression, details) = await f.CheckIfCanFixAsync(
                invocationDoc,
                RoslynTestUtils.MakeSpan(invocationSourceCode, 0),
                CancellationToken.None).ConfigureAwait(false);

            Assert.Null(invocationExpression);
            Assert.Null(details);

            f = new UsingLegacyLoggingMethodFixer
            {
                _getDeclaredSymbol = (sm, md, t) =>
                {
                    return null;
                }
            };

            (invocationExpression, details) = await f.CheckIfCanFixAsync(
                invocationDoc,
                RoslynTestUtils.MakeSpan(invocationSourceCode, 0),
                CancellationToken.None).ConfigureAwait(false);

            Assert.NotNull(invocationExpression);
            Assert.NotNull(details);

            (methodName, existing) = await f.GetFinalTargetMethodNameAsync(
                targetDoc,
                targetClass!,
                invocationDoc,
                invocationExpression!,
                details!,
                CancellationToken.None).ConfigureAwait(false);

            Assert.Equal("TestA", methodName);
            Assert.False(existing);

            f = new UsingLegacyLoggingMethodFixer
            {
                _getTypeByMetadataName1 = (c, t) =>
                {
                    return null;
                }
            };

            (invocationExpression, details) = await f.CheckIfCanFixAsync(
                invocationDoc,
                RoslynTestUtils.MakeSpan(invocationSourceCode, 0),
                CancellationToken.None).ConfigureAwait(false);

            Assert.Null(invocationExpression);
            Assert.Null(details);

            f = new UsingLegacyLoggingMethodFixer
            {
                _getTypeByMetadataName2 = (c, t) =>
                {
                    return null;
                }
            };

            (invocationExpression, details) = await f.CheckIfCanFixAsync(
                invocationDoc,
                RoslynTestUtils.MakeSpan(invocationSourceCode, 0),
                CancellationToken.None).ConfigureAwait(false);

            Assert.NotNull(invocationExpression);
            Assert.NotNull(details);

            (methodName, existing) = await f.GetFinalTargetMethodNameAsync(
                targetDoc,
                targetClass!,
                invocationDoc,
                invocationExpression!,
                details!,
                CancellationToken.None).ConfigureAwait(false);

            Assert.Equal("TestA", methodName);
            Assert.False(existing);
        }

        [Fact]
        public void ArgCheck()
        {
            var a = new UsingLegacyLoggingMethodAnalyzer();
            Assert.Throws<ArgumentNullException>(() => a.Initialize(null!));
        }
    }
}
