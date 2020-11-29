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
                // shouldn't happen, we only get called for methods on this type
                return (null, null);
            }

            var invocationOp = sm.GetOperation(invocationExpression, cancellationToken) as IInvocationOperation;
            if (invocationOp == null) 
            {
                // shouldn't happen, we're dealing with an invocation expression
                return (null, null);
            }

            var method = invocationOp.TargetMethod;
            if (method == null)
            {
                // shouldn't happen, we should only be called with a known target method
                return (null, null);
            }

            var details = new FixDetails(method, invocationOp, invocationDoc.Project.DefaultNamespace, invocationDoc.Project.Documents);

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

            // determine the final name of the logging method and whether we need to generate it or not
            var (methodName, existing) = await GetFinalTargetMethodName(targetDoc, targetClass, invocationDoc, invocationExpression, details, cancellationToken).ConfigureAwait(false);

            // if the target method doesn't already exist, go make it
            if (!existing)
            {
                // generate the logging method signature in the target class
                sol = await InsertLoggingMethodSignature(targetDoc, targetClass, invocationDoc, invocationExpression, details, cancellationToken).ConfigureAwait(false);

                // find the doc and invocation in the current solution
                (invocationDoc, invocationExpression) = await Remap(sol, invocationDocId, invocationExpression).ConfigureAwait(false);
            }

            // rewrite the call site to invoke the generated logging method
            sol = await RewriteLoggingCall(invocationDoc, invocationExpression, details, methodName, cancellationToken).ConfigureAwait(false);

            return sol;
        }

        /// <summary>
        /// Remaps an invocation expression to a new doc
        /// </summary>
        private static async Task<(Document, InvocationExpressionSyntax)> Remap(Solution sol, DocumentId docId, InvocationExpressionSyntax invocationExpression)
        {
            var doc = sol.GetDocument(docId)!;
            var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);

            return (doc, (root!.FindNode(invocationExpression.Span) as InvocationExpressionSyntax)!);
        }

        /// <summary>
        /// Finds the class into which to create the logging method signature, or creates it if it doesn't exist
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

        /// <summary>
        /// Get the final name of the target method. If there's an existing method with the right 
        /// message, level, and argument types, we just use that. Otherwise, we create a new method.
        /// </summary>
        internal static async Task<(string methodName, bool existing)> GetFinalTargetMethodName(
            Document targetDoc,
            ClassDeclarationSyntax targetClass,
            Document invocationDoc,
            InvocationExpressionSyntax invocationExpression,
            FixDetails details,
            CancellationToken cancellationToken)
        {
            var invocationSM = (await invocationDoc.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;
            var invocationOp = (invocationSM.GetOperation(invocationExpression, cancellationToken) as IInvocationOperation)!;

            var docEditor = await DocumentEditor.CreateAsync(targetDoc, cancellationToken).ConfigureAwait(false);
            var sm = docEditor.SemanticModel;
            var comp = sm.Compilation;
            var gen = docEditor.Generator;

            var loggerMessageAttribute = comp.GetTypeByMetadataName("Microsoft.Extensions.Logging.LoggerMessageAttribute");
            if (loggerMessageAttribute is null)
            {
                // strange we can't find the attribute, but supply a potential useful value instead
                return (details.TargetMethodName, false);
            }

            var conflict = false;
            foreach (var method in targetClass.Members.Where(m => m.IsKind(SyntaxKind.MethodDeclaration)).OfType<MethodDeclarationSyntax>())
            {
                var sym = sm.GetDeclaredSymbol(method, cancellationToken);
                var methodSymbol = (sym as IMethodSymbol)!;

                var matchName = (method.Identifier.ToString() == details.TargetMethodName);

                var matchParams = invocationOp.Arguments.Length == methodSymbol.Parameters.Length;
                if (matchParams)
                {
                    for (int i = 0; i < invocationOp.Arguments.Length; i++)
                    {
                        matchParams = invocationOp.Arguments[i].Type.Equals(methodSymbol.Parameters[i].Type, SymbolEqualityComparer.Default);
                        if (!matchParams)
                        {
                            break;
                        }
                    }
                }

                if (matchName && matchParams)
                {
                    conflict = true;
                }

                foreach (var mal in method.AttributeLists)
                {
                    foreach (var ma in mal.Attributes)
                    {
                        var maSymbolInfo = sm.GetSymbolInfo(ma, cancellationToken);
                        if (maSymbolInfo.Symbol is IMethodSymbol ms && loggerMessageAttribute.Equals(ms.ContainingType, SymbolEqualityComparer.Default))
                        {
                            var arg = ma.ArgumentList!.Arguments[1];
                            var level = (int)sm.GetConstantValue(arg.Expression, cancellationToken).Value!;

                            arg = ma.ArgumentList.Arguments[2];
                            var message = sm.GetConstantValue(arg.Expression, cancellationToken).ToString();

                            var matchMessage = (message == details.Message);
                            var matchLevel = level switch
                            {
                                0 => details.Level == "Trace",
                                1 => details.Level == "Debug",
                                2 => details.Level == "Information",
                                3 => details.Level == "Warning",
                                4 => details.Level == "Error",
                                5 => details.Level == "Critical",
                                _ => false,
                            };

                            if (matchLevel && matchMessage && matchParams)
                            {
                                // found a match, use this one
                                return (method.Identifier.ToString(), true);
                            }

                            break;
                        }
                    }
                }
            }

            if (conflict)
            {
                // can't use the target name, since it conflicts with something else
                return (details.TargetMethodName + "42", false);
            }

            return (details.TargetMethodName, false);
        }

        internal static async Task<Solution> InsertLoggingMethodSignature(
            Document targetDoc,
            ClassDeclarationSyntax targetClass,
            Document invocationDoc,
            InvocationExpressionSyntax invocationExpression,
            FixDetails details,
            CancellationToken cancellationToken)
        {
            var invocationSM = (await invocationDoc.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false))!;
            var invocationOp = (invocationSM.GetOperation(invocationExpression, cancellationToken) as IInvocationOperation)!;

            var solEditor = new SolutionEditor(targetDoc.Project.Solution);
            var docEditor = await solEditor.GetDocumentEditorAsync(targetDoc.Id, cancellationToken).ConfigureAwait(false);
            var sm = docEditor.SemanticModel;
            var comp = sm.Compilation;
            var gen = docEditor.Generator;

            var parameters = new List<SyntaxNode>();

            parameters.Add(gen.ParameterDeclaration("logger", gen.TypeExpression(invocationOp.Arguments[0].Value.Type)));
            if (details.ExceptionParamIndex >= 0)
            {
                parameters.Add(gen.ParameterDeclaration("exception", gen.TypeExpression(invocationOp.Arguments[details.ExceptionParamIndex].Value.Type)));
            }

            var paramsArg = invocationOp.Arguments[details.ArgsParamIndex];
            if (paramsArg != null)
            {
                var arrayCreation = paramsArg.Value as IArrayCreationOperation;
                var index = 0;
                foreach (var e in arrayCreation!.Initializer.ElementValues)
                {
                    foreach (var d in e.Descendants())
                    {
                        string name;
                        if (index < details.MessageArgs.Count)
                        {
                            name = details.MessageArgs[index];
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
                    gen.LiteralExpression(CalcEventId(comp, targetClass, cancellationToken)),
                    gen.MemberAccessExpression(gen.TypeExpression(comp.GetTypeByMetadataName("Microsoft.Extensions.Logging.LogLevel")), details.Level),
                    gen.LiteralExpression(details.Message),
                });

            logMethod = gen.AddAttributes(logMethod, attr);

            var comment = SyntaxFactory.ParseLeadingTrivia($@"
/// <summary>
/// Logs `{EscapeMessageString(details.Message)}` at `{details.Level}` level.
/// </summary>
");
            logMethod = logMethod.WithLeadingTrivia(comment);

            docEditor.AddMember(targetClass, logMethod);

            return solEditor.GetChangedSolution();
        }

        private static IReadOnlyList<SyntaxNode> MakeParameterList(
            FixDetails details,
            IInvocationOperation invocationOp,
            SyntaxGenerator gen)
        {
            var parameters = new List<SyntaxNode>();

            parameters.Add(gen.ParameterDeclaration("logger", gen.TypeExpression(invocationOp.Arguments[0].Value.Type)));
            if (details.ExceptionParamIndex >= 0)
            {
                parameters.Add(gen.ParameterDeclaration("exception", gen.TypeExpression(invocationOp.Arguments[details.ExceptionParamIndex].Value.Type)));
            }

            var paramsArg = invocationOp.Arguments[details.ArgsParamIndex];
            if (paramsArg != null)
            {
                var arrayCreation = paramsArg.Value as IArrayCreationOperation;
                var index = 0;
                foreach (var e in arrayCreation!.Initializer.ElementValues)
                {
                    foreach (var d in e.Descendants())
                    {
                        string name;
                        if (index < details.MessageArgs.Count)
                        {
                            name = details.MessageArgs[index];
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

            return parameters;
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

        private static async Task<Solution> RewriteLoggingCall(
            Document doc,
            InvocationExpressionSyntax invocationExpression,
            FixDetails details,
            string methodName,
            CancellationToken cancellationToken)
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
                gen.MemberAccessExpression(gen.TypeExpression(comp.GetTypeByMetadataName(details.FullTargetClassName)), methodName),
                argList);

            docEditor.ReplaceNode(invocationExpression, call.WithTriviaFrom(invocationExpression));

            return solEditor.GetChangedSolution();
        }

        private static string EscapeMessageString(string message)
        {
            return message
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\"", "\\\"");
        }
    }
}
