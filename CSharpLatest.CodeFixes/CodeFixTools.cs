namespace CSharpLatest;

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

internal static class CodeFixTools
{
    public static async Task<(Microsoft.CodeAnalysis.Diagnostic, T)> FindNodeToFix<T>(CodeFixContext context)
        where T : CSharpSyntaxNode
    {
        var Root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var Diagnostic = context.Diagnostics.First();
        var DiagnosticSpan = Diagnostic.Location.SourceSpan;

        // Find the type declaration identified by the diagnostic.
        var Node = Root?.FindToken(DiagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<T>().First();

        Debug.Assert(Node is not null);

        return (Diagnostic, Node!);
    }
}
