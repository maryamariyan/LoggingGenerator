// © Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[assembly: System.Resources.NeutralResourcesLanguage("en-us")]
[assembly: InternalsVisibleTo("Microsoft.Extensions.Logging.Generators.Test")]

namespace Microsoft.Extensions.Logging.Generators
{
    [Generator]
    public partial class LoggerMessageGenerator : ISourceGenerator
    {
        [ExcludeFromCodeCoverage]
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(SyntaxReceiver.Create);
        }

        [ExcludeFromCodeCoverage]
        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = context.SyntaxReceiver as SyntaxReceiver;
            if (receiver == null || receiver.ClassDeclarations.Count == 0)
            {
                // nothing to do yet
                return;
            }

            var pascalCaseArguments = false;
            var fieldName = "_logger";
            var emitDefaultMessage = true;

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("PascalCaseArguments", out var pca))
            {
                pascalCaseArguments = (pca.ToUpperInvariant() == "TRUE") || (pca.ToUpperInvariant() == "YES");
            }

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("FieldName", out var fn))
            {
                fieldName = fn;
            }

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("EmitDefaultMessage", out var edm))
            {
                emitDefaultMessage = (edm.ToUpperInvariant() == "TRUE") || (edm.ToUpperInvariant() == "YES");
            }

            var p = new Parser(context.Compilation, context.ReportDiagnostic, context.CancellationToken);
            var e = new Emitter(pascalCaseArguments, fieldName, emitDefaultMessage);
            var logClasses = p.GetLogClasses(receiver.ClassDeclarations);
            var result = e.Emit(logClasses, context.CancellationToken);

            context.AddSource(nameof(LoggerMessageGenerator), SourceText.From(result, Encoding.UTF8));
        }

        [ExcludeFromCodeCoverage]
        private sealed class SyntaxReceiver : ISyntaxReceiver
        {
            internal static ISyntaxReceiver Create()
            {
                return new SyntaxReceiver();
            }

            public List<ClassDeclarationSyntax> ClassDeclarations { get; } = new ();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classSyntax)
                {
                    ClassDeclarations.Add(classSyntax);
                }
            }
        }
    }
}
