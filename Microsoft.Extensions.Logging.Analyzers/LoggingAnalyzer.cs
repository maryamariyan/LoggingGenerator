// © Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

[assembly: System.Resources.NeutralResourcesLanguage("en-us")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.Extensions.Logging.Analyzers.Tests")]

namespace Microsoft.Extensions.Logging.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LoggingAnalyzer : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "LA0000";
        private const string DiagnosticCategory = "Performance";

#pragma warning disable RS2008 // Enable analyzer release tracking

        private static readonly DiagnosticDescriptor UsingLegacyLoggingMethod = new(
            id: DiagnosticId,
            title: Resources.UsingLegacyMethodTitle,
            messageFormat: Resources.UsingLegacyMethodMessage,
            category: DiagnosticCategory,
            description: Resources.UsingLegacyMethodDescription,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(UsingLegacyLoggingMethod);

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var loggerExtensions = compilationStartContext.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.LoggerExtensions");
                var legacyMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
                
                if (loggerExtensions != null)
                {
                    legacyMethods.UnionWith(loggerExtensions.GetMembers("LogTrace").OfType<IMethodSymbol>());
                    legacyMethods.UnionWith(loggerExtensions.GetMembers("LogDebug").OfType<IMethodSymbol>());
                    legacyMethods.UnionWith(loggerExtensions.GetMembers("LogInformation").OfType<IMethodSymbol>());
                    legacyMethods.UnionWith(loggerExtensions.GetMembers("LogWarning").OfType<IMethodSymbol>());
                    legacyMethods.UnionWith(loggerExtensions.GetMembers("LogError").OfType<IMethodSymbol>());
                    legacyMethods.UnionWith(loggerExtensions.GetMembers("LogCritical").OfType<IMethodSymbol>());
                    legacyMethods.UnionWith(loggerExtensions.GetMembers("Log").OfType<IMethodSymbol>());
                }

                compilationStartContext.RegisterOperationBlockStartAction(operationBlockContext =>
                {
                    if (operationBlockContext.OwningSymbol.Kind != SymbolKind.Method)
                    {
                        return;
                    }

                    operationBlockContext.RegisterOperationAction(operationContext =>
                    {
                        var invocationOp = (IInvocationOperation)operationContext.Operation;
                        var method = invocationOp.TargetMethod;

                        if (legacyMethods.Contains(method.OriginalDefinition))
                        {
                            var diagnostic = Diagnostic.Create(UsingLegacyLoggingMethod, invocationOp.Syntax.GetLocation());
                            operationContext.ReportDiagnostic(diagnostic);
                        }
                    }, OperationKind.Invocation);
                });
            });
        }
    }
}
