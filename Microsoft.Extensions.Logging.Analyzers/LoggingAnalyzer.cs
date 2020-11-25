// © Microsoft Corporation. All rights reserved.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.Extensions.Logging.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LoggingAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "LA0000";
        private const string DiagnosticCategory = "Performance";

#pragma warning disable RS2008 // Enable analyzer release tracking

        private static readonly DiagnosticDescriptor UsingLegacyLoggingMethod = new(
            id: DiagnosticId,
            title: new LocalizableResourceString(nameof(Resources.UsingLegacyMethodTitle), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.UsingLegacyMethodMessage), Resources.ResourceManager, typeof(Resources)),
            category: DiagnosticCategory,
            description: new LocalizableResourceString(nameof(Resources.UsingLegacyMethodDescription), Resources.ResourceManager, typeof(Resources)),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(UsingLegacyLoggingMethod);

        public override void Initialize(AnalysisContext context)
        {
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
                }

                compilationStartContext.RegisterOperationBlockStartAction(operationBlockContext =>
                {
                    if (operationBlockContext.OwningSymbol.Kind != SymbolKind.Method)
                    {
                        return;
                    }

                    operationBlockContext.RegisterOperationAction(operationContext =>
                    {
                        var invocation = (IInvocationOperation)operationContext.Operation;
                        var method = invocation.TargetMethod;
                        if (method == null)
                        {
                            return;
                        }

                        if (legacyMethods.Contains(invocation.TargetMethod.OriginalDefinition))
                        {
                            var diagnostic = Diagnostic.Create(UsingLegacyLoggingMethod, invocation.Syntax.GetLocation());
                            operationContext.ReportDiagnostic(diagnostic);
                        }
                    }, OperationKind.Invocation);
                });
            });
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
        }
    }
}
