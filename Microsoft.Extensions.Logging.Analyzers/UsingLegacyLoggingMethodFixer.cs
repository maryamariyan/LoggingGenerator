// © Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Extensions.Logging.Analyzers
{
    // This rule doesn't agree with non-nullable syntax
#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly

    // I want specialized type parameters, they show intent
#pragma warning disable S3242 // Method parameters should be declared with base types

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsingLegacyLoggingMethodFixer))]
    [Shared]
    public sealed partial class UsingLegacyLoggingMethodFixer : CodeFixProvider
    {
#pragma warning disable SA1401 // Fields should be private
        // function pointers that can be patched by test code to exercise obscure failure paths
        internal Func<Document, CancellationToken, Task<SyntaxNode?>> _getSyntaxRootAsync = (d, t) => d.GetSyntaxRootAsync(t);
        internal Func<Document, CancellationToken, Task<SemanticModel?>> _getSemanticModelAsync = (d, t) => d.GetSemanticModelAsync(t);
        internal Func<SemanticModel, SyntaxNode, CancellationToken, IOperation?> _getOperation = (sm, sn, t) => sm.GetOperation(sn, t);
        internal Func<Compilation, string, INamedTypeSymbol?> _getTypeByMetadataName1 = (c, n) => c.GetTypeByMetadataName(n);
        internal Func<Compilation, string, INamedTypeSymbol?> _getTypeByMetadataName2 = (c, n) => c.GetTypeByMetadataName(n);
        internal Func<Compilation, string, INamedTypeSymbol?> _getTypeByMetadataName3 = (c, n) => c.GetTypeByMetadataName(n);
        internal Func<SemanticModel, BaseMethodDeclarationSyntax, CancellationToken, IMethodSymbol?> _getDeclaredSymbol = (sm, m, t) => sm.GetDeclaredSymbol(m, t);
#pragma warning restore SA1401 // Fields should be private

        private const string LoggerMessageAttribute = "Microsoft.Extensions.Logging.LoggerMessageAttribute";
        private const int LoggerMessageAttrEventIdArg = 0;
        private const int LoggerMessageAttrLevelArg = 1;
        private const int LoggerMessageAttrMessageArg = 2;

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagDescriptors.UsingLegacyLoggingMethod.Id);
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var (invocationExpression, details) = await CheckIfCanFixAsync(context.Document, context.Span, context.CancellationToken).ConfigureAwait(false);
            if (invocationExpression != null && details != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: Resources.GenerateStronglyTypedLoggingMethod,
                        createChangedSolution: cancellationToken => ApplyFixAsync(context.Document, invocationExpression, details, cancellationToken),
                        equivalenceKey: nameof(Resources.GenerateStronglyTypedLoggingMethod)),
                    context.Diagnostics);
            }
        }

        internal async Task<(InvocationExpressionSyntax?, FixDetails?)> CheckIfCanFixAsync(Document invocationDoc, TextSpan span, CancellationToken cancellationToken)
        {
            var root = await _getSyntaxRootAsync(invocationDoc, cancellationToken).ConfigureAwait(false);
            if (root == null || root.FindNode(span) is not InvocationExpressionSyntax invocationExpression)
            {
                // shouldn't happen, we only get called for invocations
                return (null, null);
            }

            var sm = await _getSemanticModelAsync(invocationDoc, cancellationToken).ConfigureAwait(false);
            if (sm == null)
            {
                // shouldn't happen
                return (null, null);
            }

            var comp = sm.Compilation;

            var loggerExtensions = _getTypeByMetadataName1(comp, "Microsoft.Extensions.Logging.LoggerExtensions");
            if (loggerExtensions == null)
            {
                // shouldn't happen, we only get called for methods on this type
                return (null, null);
            }

            var invocationOp = _getOperation(sm, invocationExpression, cancellationToken) as IInvocationOperation;
            if (invocationOp == null)
            {
                // shouldn't happen, we're dealing with an invocation expression
                return (null, null);
            }

            var method = invocationOp.TargetMethod;

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

        /// <summary>
        /// Orchestrate all the work needed to fix an issue.
        /// </summary>
        internal async Task<Solution> ApplyFixAsync(Document invocationDoc, InvocationExpressionSyntax invocationExpression, FixDetails details, CancellationToken cancellationToken)
        {
            ClassDeclarationSyntax targetClass;
            Document targetDoc;
            Solution sol;

            // stable id surviving across solution generations
            var invocationDocId = invocationDoc.Id;

            // get a reference to the class where to insert the logging method, creating it if necessary
            (sol, targetClass, targetDoc) = await GetOrMakeTargetClassAsync(invocationDoc.Project, details, cancellationToken).ConfigureAwait(false);

            // find the doc and invocation in the current solution
            (invocationDoc, invocationExpression) = await RemapAsync(sol, invocationDocId, invocationExpression).ConfigureAwait(false);

            // determine the final name of the logging method and whether we need to generate it or not
            var (methodName, existing) = await GetFinalTargetMethodNameAsync(targetDoc, targetClass, invocationDoc, invocationExpression, details, cancellationToken).ConfigureAwait(false);

            // if the target method doesn't already exist, go make it
            if (!existing)
            {
                // generate the logging method signature in the target class
                sol = await InsertLoggingMethodSignatureAsync(targetDoc, targetClass, invocationDoc, invocationExpression, details, cancellationToken).ConfigureAwait(false);

                // find the doc and invocation in the current solution
                (invocationDoc, invocationExpression) = await RemapAsync(sol, invocationDocId, invocationExpression).ConfigureAwait(false);
            }

            // rewrite the call site to invoke the generated logging method
            sol = await RewriteLoggingCallAsync(invocationDoc, invocationExpression, details, methodName, cancellationToken).ConfigureAwait(false);

            return sol;
        }

        /// <summary>
        /// Get the final name of the target method. If there's an existing method with the right
        /// message, level, and argument types, we just use that. Otherwise, we create a new method.
        /// </summary>
        internal async Task<(string methodName, bool existing)> GetFinalTargetMethodNameAsync(
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

            var loggerMessageAttribute = _getTypeByMetadataName2(comp, LoggerMessageAttribute);
            if (loggerMessageAttribute is null)
            {
                // strange that we can't find the attribute, but supply a potential useful value instead
                return (details.TargetMethodName, false);
            }

            var invocationArgList = MakeArgumentList(details, invocationOp);

            var conflict = false;
            var count = 2;
            var methodName = string.Empty;
            do
            {
                methodName = details.TargetMethodName;
                if (conflict)
                {
                    methodName = $"{methodName}{count}";
                    count++;
                    conflict = false;
                }

                foreach (var method in targetClass.Members.Where(m => m.IsKind(SyntaxKind.MethodDeclaration)).OfType<MethodDeclarationSyntax>())
                {
                    var methodSymbol = _getDeclaredSymbol(sm, method, cancellationToken);
                    if (methodSymbol == null)
                    {
                        // hmmm, this shouldn't happen should it?
                        continue;
                    }

                    var matchName = method.Identifier.ToString() == methodName;

                    var matchParams = invocationArgList.Count == methodSymbol.Parameters.Length;
                    if (matchParams)
                    {
                        for (int i = 0; i < invocationArgList.Count; i++)
                        {
                            matchParams = invocationArgList[i].Equals(methodSymbol.Parameters[i].Type, SymbolEqualityComparer.Default);
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
                            var mattrSymbolInfo = sm.GetSymbolInfo(ma, cancellationToken);
                            if (mattrSymbolInfo.Symbol is IMethodSymbol ms)
                            {
                                if (loggerMessageAttribute.Equals(ms.ContainingType, SymbolEqualityComparer.Default))
                                {
                                    var arg = ma.ArgumentList!.Arguments[LoggerMessageAttrLevelArg];
                                    var level = (LogLevel)sm.GetConstantValue(arg.Expression, cancellationToken).Value!;

                                    arg = ma.ArgumentList.Arguments[LoggerMessageAttrMessageArg];
                                    var message = sm.GetConstantValue(arg.Expression, cancellationToken).ToString();

                                    var matchMessage = message == details.Message;
                                    var matchLevel = FixDetails.GetLogLevelName(level) == details.Level;

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
                }
            }
            while (conflict);

            return (methodName, false);
        }

        /// <summary>
        /// Finds the class into which to create the logging method signature, or creates it if it doesn't exist.
        /// </summary>
        private static async Task<(Solution, ClassDeclarationSyntax, Document)> GetOrMakeTargetClassAsync(Project proj, FixDetails details, CancellationToken cancellationToken)
        {
            while (true)
            {
                var comp = (await proj.GetCompilationAsync(cancellationToken).ConfigureAwait(false))!;
                var allNodes = comp.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
                var allClasses = allNodes.Where(d => d.IsKind(SyntaxKind.ClassDeclaration)).OfType<ClassDeclarationSyntax>();
                foreach (var cl in allClasses)
                {
                    var nspace = GetNamespace(cl);
                    if (nspace != details.TargetNamespace)
                    {
                        continue;
                    }

                    if (cl.Identifier.Text == details.TargetClassName)
                    {
                        return (proj.Solution, cl, proj.GetDocument(cl.SyntaxTree)!);
                    }
                }

                var text = $@"
#pragma warning disable CS8019
using Microsoft.Extensions.Logging;
using System;
#pragma warning restore CS8019

static partial class {details.TargetClassName}
{{
}}
";

                if (!string.IsNullOrEmpty(details.TargetNamespace))
                {
                    text = $@"
namespace {details.TargetNamespace}
{{
#pragma warning disable CS8019
    using Microsoft.Extensions.Logging;
    using System;
#pragma warning restore CS8019

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
        /// Remaps an invocation expression to a new doc
        /// </summary>
        private static async Task<(Document, InvocationExpressionSyntax)> RemapAsync(Solution sol, DocumentId docId, InvocationExpressionSyntax invocationExpression)
        {
            var doc = sol.GetDocument(docId)!;
            var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);

            return (doc, (root!.FindNode(invocationExpression.Span) as InvocationExpressionSyntax)!);
        }

        private static string GetNamespace(ClassDeclarationSyntax cl)
        {
            var ns = cl.Parent as NamespaceDeclarationSyntax;
            if (ns == null)
            {
                if (cl.Parent is not CompilationUnitSyntax)
                {
                    // nested type, we don't do those
                    return "<+Invalid Namespace+>";
                }

                return string.Empty;
            }

            var nspace = ns.Name.ToString();
            while (true)
            {
                ns = ns.Parent as NamespaceDeclarationSyntax;
                if (ns == null)
                {
                    break;
                }

                nspace = $"{ns.Name}.{nspace}";
            }

            return nspace;
        }

        /// <summary>
        /// Given a LoggerExtensions method invocation, produce a parameter list for the corresponding generated logging method
        /// </summary>
        private static IReadOnlyList<SyntaxNode> MakeParameterList(
            FixDetails details,
            IInvocationOperation invocationOp,
            SyntaxGenerator gen)
        {
            var parameters = new List<SyntaxNode>
            {
                gen.ParameterDeclaration("logger", gen.TypeExpression(invocationOp.Arguments[0].Value.Type))
            };

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
        /// Given a LoggerExtensions method invocation, produce an argument list in the shape of a corresponding generated logging method
        /// </summary>
        private static IReadOnlyList<ITypeSymbol> MakeArgumentList(FixDetails details, IInvocationOperation invocationOp)
        {
            var args = new List<ITypeSymbol>
            {
                invocationOp.Arguments[0].Value.Type
            };

            if (details.ExceptionParamIndex >= 0)
            {
                args.Add(invocationOp.Arguments[details.ExceptionParamIndex].Value.Type);
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
                        args.Add(d.Type);
                        index++;
                    }
                }
            }

            return args;
        }

        private static async Task<Solution> RewriteLoggingCallAsync(
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

            int index = 0;
            foreach (var arg in invocation!.Arguments)
            {
                if ((index == details.MessageParamIndex) || (index == details.LogLevelParamIndex))
                {
                    index++;
                    continue;
                }

                index++;

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

            var typeSyntax = comp.GetTypeByMetadataName(details.FullTargetClassName);
            var typeSymbol = gen.TypeExpression(typeSyntax);
            var memberAccessExpression = gen.MemberAccessExpression(typeSymbol, methodName);
            var call = gen.InvocationExpression(memberAccessExpression, argList);

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

        private async Task<Solution> InsertLoggingMethodSignatureAsync(
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

            var logMethod = gen.MethodDeclaration(
                                details.TargetMethodName,
                                MakeParameterList(details, invocationOp, gen),
                                accessibility: Accessibility.Internal,
                                modifiers: DeclarationModifiers.Partial | DeclarationModifiers.Static);

            var attrArgs = new[]
            {
                gen.LiteralExpression(CalcEventId(comp, targetClass, cancellationToken)),
                gen.MemberAccessExpression(gen.TypeExpression(comp.GetTypeByMetadataName("Microsoft.Extensions.Logging.LogLevel")), details.Level),
                gen.LiteralExpression(details.Message),
            };

            var attr = gen.Attribute(
                gen.TypeExpression(comp.GetTypeByMetadataName(LoggerMessageAttribute)),
                attrArgs);

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

        /// <summary>
        /// Iterate through the existing methods in the target class
        /// and look at any method annotated with [LoggerMessage],
        /// get their event ids, and then return 1 larger than any event id
        /// found.
        /// </summary>
        private int CalcEventId(Compilation comp, ClassDeclarationSyntax targetClass, CancellationToken cancellationToken)
        {
            var loggerMessageAttribute = _getTypeByMetadataName3(comp, LoggerMessageAttribute);
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
                        var mattrSymbol = sm.GetSymbolInfo(ma, cancellationToken);
                        if (mattrSymbol.Symbol is IMethodSymbol ms)
                        {
                            if (loggerMessageAttribute.Equals(ms.ContainingType, SymbolEqualityComparer.Default))
                            {
                                var arg = ma.ArgumentList!.Arguments[LoggerMessageAttrEventIdArg];
                                var eventId = (int)(sm.GetConstantValue(arg.Expression, cancellationToken).Value!);
                                if (eventId >= max)
                                {
                                    max = eventId + 1;
                                }
                            }
                        }
                    }
                }
            }

            return max;
        }
    }
}
