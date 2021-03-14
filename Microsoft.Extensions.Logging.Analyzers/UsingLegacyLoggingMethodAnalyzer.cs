// © Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

[assembly: System.Resources.NeutralResourcesLanguage("en-us")]
[assembly: InternalsVisibleTo("Microsoft.Extensions.Logging.Analyzers.Test")]

namespace Microsoft.Extensions.Logging.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UsingLegacyLoggingMethodAnalyzer : DiagnosticAnalyzer
    {
#pragma warning disable RS2008 // Enable analyzer release tracking

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagDescriptors.UsingLegacyLoggingMethod);

        public override void Initialize(AnalysisContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var loggerExtensions = compilationStartContext.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.LoggerExtensions");

                if (loggerExtensions != null)
                {
#pragma warning disable RS1024 // Compare symbols correctly
                    var legacyMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly

                    legacyMethods.UnionWith(loggerExtensions.GetMembers("LogTrace").OfType<IMethodSymbol>());
                    legacyMethods.UnionWith(loggerExtensions.GetMembers("LogDebug").OfType<IMethodSymbol>());
                    legacyMethods.UnionWith(loggerExtensions.GetMembers("LogInformation").OfType<IMethodSymbol>());
                    legacyMethods.UnionWith(loggerExtensions.GetMembers("LogWarning").OfType<IMethodSymbol>());
                    legacyMethods.UnionWith(loggerExtensions.GetMembers("LogError").OfType<IMethodSymbol>());
                    legacyMethods.UnionWith(loggerExtensions.GetMembers("LogCritical").OfType<IMethodSymbol>());
                    legacyMethods.UnionWith(loggerExtensions.GetMembers("Log").OfType<IMethodSymbol>());

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
                                var diagnostic = Diagnostic.Create(DiagDescriptors.UsingLegacyLoggingMethod, invocationOp.Syntax.GetLocation());
                                operationContext.ReportDiagnostic(diagnostic);
                            }
                        }, OperationKind.Invocation);
                    });
                }
            });
        }
    }
}
