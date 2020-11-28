// © Microsoft Corporation. All rights reserved.

namespace Microsoft.Extensions.Logging.Analyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Operations;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LoggingFixes)), Shared]
    public class LoggingFixes : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(LoggingAnalyzer.DiagnosticId);
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var (invocationExpression, details) = await CheckIfCanFix(context.Document, context.Span, context.CancellationToken).ConfigureAwait(false);
            if (invocationExpression != null && details != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: Resources.GenerateStronglyTypedLoggingMethod,
                        createChangedSolution: cancellationToken => ApplyFix(context.Document, invocationExpression, details, cancellationToken),
                        equivalenceKey: nameof(Resources.GenerateStronglyTypedLoggingMethod)),
                    context.Diagnostics);
            }
        }

        internal static async Task<(InvocationExpressionSyntax?, FixDetails?)> CheckIfCanFix(Document invocationDoc, TextSpan span, CancellationToken cancellationToken)
        {
            var root = await invocationDoc.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null || root.FindNode(span) is not InvocationExpressionSyntax invocationExpression)
            {
                // shouldn't happen, we only get called for invocations
                return (null, null);
            }

            var sm = await invocationDoc.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (sm == null)
            {
                // shouldn't happen
                return (null, null);
            }

            var comp = sm.Compilation;

            var loggerExtensions = comp.GetTypeByMetadataName("Microsoft.Extensions.Logging.LoggerExtensions");
            if (loggerExtensions == null)
            {
                // should't happen, we only get called for calls on this type
                return (null, null);
            }

            var op = sm.GetOperation(invocationExpression, cancellationToken);
            if (op is not IInvocationOperation invocation)
            {
                // shouldn't happen, we're dealing with an invocation expression
                return (null, null);
            }

            var method = invocation.TargetMethod;
            if (method == null)
            {
                // shouldn't happen, we should only be called with a known target method
                return (null, null);
            }

            var details = new FixDetails(method, invocation, invocationDoc.Project.DefaultNamespace);

            if (string.IsNullOrWhiteSpace(details.Message))
            {
                // can't auto-generate without a valid message string
                return (null, null);
            }

            if (details.EventIdParamIndex >= 0)
            {
                // can't auto-generate the variants using event id
                return (null, null);
            }

            if (string.IsNullOrWhiteSpace(details.Level))
            {
                // can't auto-generate without a valid level
                return (null, null);
            }

            return (invocationExpression, details);
        }

        internal static async Task<Solution> ApplyFix(Document invocationDoc, InvocationExpressionSyntax invocationExpression, FixDetails details, CancellationToken cancellationToken)
        {
            ClassDeclarationSyntax targetClass;
            Document targetDoc;
            Solution sol;
            Solution originalSolution = invocationDoc.Project.Solution;

            // stable id surviving across solution generations
            var invocationDocId = invocationDoc.Id;

            // get a reference to the class where to insert the logging method, creating it if necessary
            (sol, targetClass, targetDoc) = await GetOrMakeTargetClass(invocationDoc.Project, details, cancellationToken).ConfigureAwait(false);

            // find the doc and invocation in the current solution
            (invocationDoc, invocationExpression) = await Remap(sol, invocationDocId, invocationExpression).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                return originalSolution;
            }

            // generate the logging method signature in the target class
            sol = await InsertLoggingMethodSignature(targetDoc, targetClass, invocationDoc, invocationExpression, details, cancellationToken).ConfigureAwait(false);

            // find the doc and invocation in the current solution
            (invocationDoc, invocationExpression) = await Remap(sol, invocationDocId, invocationExpression).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                return originalSolution;
            }

            // rewrite the call site to invoke the generated logging method
            sol = await RewriteLoggingCall(invocationDoc, invocationExpression, details, cancellationToken).ConfigureAwait(false);

            return sol;
        }

        /// <summary>
        /// Remaps an a invocation expression to a new doc
        /// </summary>
        private static async Task<(Document, InvocationExpressionSyntax)> Remap(Solution sol, DocumentId docId, InvocationExpressionSyntax invocationExpression)
        {
            var doc = sol.GetDocument(docId)!;
            var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);

            return (doc, (root!.FindNode(invocationExpression.Span) as InvocationExpressionSyntax)!);
        }

        /// <summary>
        /// FInds the class into which to create the logging method signature, or creates it if it doesn't exist
        /// </summary>
        internal static async Task<(Solution, ClassDeclarationSyntax, Document)> GetOrMakeTargetClass(Project proj, FixDetails details, CancellationToken cancellationToken)
        {
            for (; ; )
            {
                var sm = (await proj.Documents.First().GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;

                var allNodes = sm.Compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
                var allClasses = allNodes.Where(d => d.IsKind(SyntaxKind.ClassDeclaration)).OfType<ClassDeclarationSyntax>();
                foreach (var cl in allClasses)
                {
                    if (details.TargetNamespace != null)
                    {
                        var parent = cl.Parent as NamespaceDeclarationSyntax;
                        if (parent == null || parent.Name.ToString() != details.TargetNamespace)
                        {
                            continue;
                        }
                    }

                    if (cl.Identifier.Text == details.TargetClassName)
                    {
                        return (proj.Solution, cl, proj.GetDocument(cl.SyntaxTree)!);
                    }
                }

                var text = $@"
using Microsoft.Extensions.Logging;
using System;

static partial class {details.TargetClassName}
{{
}}
";

                if (details.TargetNamespace != null)
                {
                    text = $@"
using Microsoft.Extensions.Logging;
using System;

namespace {details.TargetNamespace}
{{
    static partial class {details.TargetClassName}
    {{
    }}
}}
";
                }

                proj = proj.AddDocument(details.TargetFilename, text).Project;
            }
        }

        internal static async Task<Solution> InsertLoggingMethodSignature(Document targetDoc, ClassDeclarationSyntax targetClass,
            Document invocationDoc, InvocationExpressionSyntax invocationExpression, FixDetails details, CancellationToken cancellationToken)
        {
            var invocationSM = (await invocationDoc.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;
            var invocation = (invocationSM.GetOperation(invocationExpression, cancellationToken) as IInvocationOperation)!;

            var solEditor = new SolutionEditor(targetDoc.Project.Solution);
            var docEditor = await solEditor.GetDocumentEditorAsync(targetDoc.Id, cancellationToken).ConfigureAwait(false);
            var sm = docEditor.SemanticModel;
            var comp = sm.Compilation;
            var gen = docEditor.Generator;

            var parameters = new List<SyntaxNode>();
            var templateArgs = LoggingTemplates.ExtractTemplateArgs(details.Message);

            int eventId = CalcEventId(comp, targetClass, cancellationToken);

            parameters.Add(gen.ParameterDeclaration("logger", gen.TypeExpression(invocation.Arguments[0].Value.Type)));
            if (details.ExceptionParamIndex >= 0)
            {
                parameters.Add(gen.ParameterDeclaration("exception", gen.TypeExpression(invocation.Arguments[details.ExceptionParamIndex].Value.Type)));
            }

            var paramsArg = invocation.Arguments[details.ArgsIndex];
            if (paramsArg != null)
            {
                var arrayCreation = paramsArg.Value as IArrayCreationOperation;
                var index = 0;
                foreach (var e in arrayCreation!.Initializer.ElementValues)
                {
                    foreach (var d in e.Descendants())
                    {
                        string name;
                        if (index < templateArgs.Count)
                        {
                            name = templateArgs[index];
                        }
                        else
                        {
                            name = $"arg{index}";
                        }

                        parameters.Add(gen.ParameterDeclaration(name, gen.TypeExpression(d.Type)));
                        index++;
                    }
                }
            }

            var logMethod = gen.MethodDeclaration(
                                details.TargetMethodName,
                                parameters,
                                accessibility: Accessibility.Internal,
                                modifiers: DeclarationModifiers.Partial | DeclarationModifiers.Static);

            var attr = gen.Attribute(
                gen.TypeExpression(comp.GetTypeByMetadataName("Microsoft.Extensions.Logging.LoggerMessageAttribute")),
                new[] {
                    gen.LiteralExpression(eventId),
                    gen.MemberAccessExpression(gen.TypeExpression(comp.GetTypeByMetadataName("Microsoft.Extensions.Logging.LogLevel")), details.Level),
                    gen.LiteralExpression(details.Message),
                });

            logMethod = gen.AddAttributes(logMethod, attr);

            docEditor.AddMember(targetClass, logMethod);

            return solEditor.GetChangedSolution();
        }

        /// <summary>
        /// Iterate through the existing methods in the target class
        /// and look at any method annotated with [LoggerMessage],
        /// get their event ids, and then return 1 larger than any event id
        /// found.
        /// </summary>
        private static int CalcEventId(Compilation comp, ClassDeclarationSyntax targetClass, CancellationToken cancellationToken)
        {
            var loggerMessageAttribute = comp.GetTypeByMetadataName("Microsoft.Extensions.Logging.LoggerMessageAttribute");
            if (loggerMessageAttribute is null)
            {
                // strange we can't find the attribute, but supply a potential useful value instead
                return targetClass.Members.Count + 1;
            }

            var max = 0;
            foreach (var method in targetClass.Members.Where(m => m.IsKind(SyntaxKind.MethodDeclaration)).OfType<MethodDeclarationSyntax>())
            {
                foreach (var mal in method.AttributeLists)
                {
                    foreach (var ma in mal.Attributes)
                    {
                        var sm = comp.GetSemanticModel(ma.SyntaxTree);
                        var maSymbol = sm.GetSymbolInfo(ma, cancellationToken);
                        if (maSymbol.Symbol is IMethodSymbol ms && loggerMessageAttribute.Equals(ms.ContainingType, SymbolEqualityComparer.Default))
                        {
                            var arg = ma.ArgumentList!.Arguments[0];
                            var eventId = (int)(sm.GetConstantValue(arg.Expression, cancellationToken).Value!);
                            if (eventId > max)
                            {
                                max = eventId;
                            }
                        }
                    }
                }
            }

            return max + 1;
        }

        private static async Task<Solution> RewriteLoggingCall(Document doc, InvocationExpressionSyntax invocationExpression,
            FixDetails details, CancellationToken cancellationToken)
        {
            var solEditor = new SolutionEditor(doc.Project.Solution);
            var docEditor = await solEditor.GetDocumentEditorAsync(doc.Id, cancellationToken).ConfigureAwait(false);
            var sm = docEditor.SemanticModel;
            var comp = sm.Compilation;
            var gen = docEditor.Generator;
            var invocation = sm.GetOperation(invocationExpression, cancellationToken) as IInvocationOperation;
            var argList = new List<SyntaxNode>();

            foreach (var arg in invocation!.Arguments)
            {
                if (arg.ArgumentKind == ArgumentKind.ParamArray)
                {
                    var arrayCreation = arg.Value as IArrayCreationOperation;
                    foreach (var e in arrayCreation!.Initializer.ElementValues)
                    {
                        argList.Add(e.Syntax);
                    }
                }
                else
                {
                    argList.Add(arg.Syntax);
                }
            }

            // remove the message argument
            argList.RemoveAt(details.MessageParamIndex);

            var call = gen.InvocationExpression(
                gen.MemberAccessExpression(gen.TypeExpression(comp.GetTypeByMetadataName(details.FullTargetClassName)), details.TargetMethodName),
                argList);

            docEditor.ReplaceNode(invocationExpression, call.WithTriviaFrom(invocationExpression));

            return solEditor.GetChangedSolution();
        }
    }
}
