namespace CSharpLatest;

using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class CodeFixTools
{
    public static async Task<(Diagnostic, T)> FindNodeToFix<T>(CodeFixContext context)
        where T : CSharpSyntaxNode
    {
        SyntaxNode? Root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        Debug.Assert(Root is not null);

        var Diagnostic = context.Diagnostics.First();
        var DiagnosticSpan = Diagnostic.Location.SourceSpan;

        var Parent = Root!.FindToken(DiagnosticSpan.Start).Parent;
        Debug.Assert(Parent is not null);

        // Find the type declaration identified by the diagnostic.
        var Node = Parent!.AncestorsAndSelf().OfType<T>().First();
        Debug.Assert(Node is not null);

        return (Diagnostic, Node!);
    }

    public static async Task<SemanticModel> GetSemanticModel(Document document, CancellationToken cancellationToken)
    {
        SemanticModel? SemanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        Debug.Assert(SemanticModel is not null);

        return SemanticModel!;
    }

    public static ITypeSymbol GetVarConvertedType(SemanticModel semanticModel, TypeSyntax variableTypeName, CancellationToken cancellationToken)
    {
        ITypeSymbol? VariableType = semanticModel.GetTypeInfo(variableTypeName, cancellationToken).ConvertedType;
        Debug.Assert(VariableType is not null);

        return VariableType!;
    }

    public static async Task<Document> WithReplacedNode(this Document document, CancellationToken cancellationToken, SyntaxNode oldNode, SyntaxNode newNode)
    {
        SyntaxNode? OldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        Debug.Assert(OldRoot is not null);

        SyntaxNode? NewRoot = OldRoot!.ReplaceNode(oldNode, newNode);
        Debug.Assert(NewRoot is not null);

        // Return document with transformed tree.
        return document.WithSyntaxRoot(NewRoot!);
    }
}
