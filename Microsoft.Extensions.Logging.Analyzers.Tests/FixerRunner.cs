// © Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Extensions.Logging.Analyzers.Test
{
    internal static class FixerRunner
    {
        public static async Task<(string, string)> ApplyAllFixes(
            DiagnosticAnalyzer analyzer,
            CodeFixProvider fixer,
            string primarySource,
            string? secondarySource,
            string? targetName = null,
            string? defaultNamespace = null,
            string? secondarySourceName = null)
        {
            var proj = RoslynTestUtils
                .CreateTestProject()
                    .WithLoggingBoilerplate()
                    .WithDocument(secondarySourceName ?? "secondary.cs", secondarySource ?? string.Empty)
                    .WithDocument("primary.cs", primarySource);

            if (defaultNamespace != null)
            {
                proj = proj.WithDefaultNamespace(defaultNamespace);
            }

            await proj.CommitChanges().ConfigureAwait(false);

            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

            while (true)
            {
                var comp = await proj!.GetCompilationAsync().ConfigureAwait(false);
                var diags = await comp!.WithAnalyzers(analyzers).GetAllDiagnosticsAsync().ConfigureAwait(false);
                if (diags.IsEmpty)
                {
                    // no more diagnostics reported by the analyzers
                    break;
                }

                var actions = new List<CodeAction>();
                foreach (var d in diags)
                {
                    var doc = proj.GetDocument(d.Location.SourceTree);

                    var context = new CodeFixContext(doc!, d, (action, diags) => actions.Add(action), CancellationToken.None);
                    await fixer.RegisterCodeFixesAsync(context).ConfigureAwait(false);
                }

                if (actions.Count == 0)
                {
                    // nothing to fix
                    break;
                }

                var operations = await actions[0].GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
                var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
                proj = solution.GetProject(proj.Id);
            }

            var sourceText = await proj.FindDocument("primary.cs").GetTextAsync().ConfigureAwait(false);
            var targetText = await proj.FindDocument(targetName ?? "secondary.cs").GetTextAsync().ConfigureAwait(false);

            return (sourceText.ToString().Replace("\r\n", "\n", System.StringComparison.InvariantCulture),
                targetText.ToString().Replace("\r\n", "\n", System.StringComparison.InvariantCulture));
        }
    }
}
