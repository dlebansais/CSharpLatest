namespace CSharpLatest;

using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class CodeFixTools
{
    public static async Task<(Diagnostic, T)> FindNodeToFix<T>(CodeFixContext context)
        where T : CSharpSyntaxNode
    {
        SyntaxNode Root = Contract.AssertNotNull(await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false));

        var Diagnostic = context.Diagnostics.First();
        var DiagnosticSpan = Diagnostic.Location.SourceSpan;

        SyntaxNode Parent = Contract.AssertNotNull(Root.FindToken(DiagnosticSpan.Start).Parent);

        // Find the type declaration identified by the diagnostic.
        T Node = Contract.AssertNotNull(Parent.AncestorsAndSelf().OfType<T>().First());

        return (Diagnostic, Node);
    }

    public static async Task<SemanticModel> GetSemanticModel(Document document, CancellationToken cancellationToken)
    {
        SemanticModel SemanticModel = Contract.AssertNotNull(await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false));

        return SemanticModel;
    }

    public static ITypeSymbol GetVarConvertedType(SemanticModel semanticModel, TypeSyntax variableTypeName, CancellationToken cancellationToken)
    {
        ITypeSymbol VariableType = Contract.AssertNotNull(semanticModel.GetTypeInfo(variableTypeName, cancellationToken).ConvertedType);

        return VariableType;
    }

    public static async Task<Document> WithReplacedNode(this Document document, CancellationToken cancellationToken, SyntaxNode oldNode, SyntaxNode newNode)
    {
        SyntaxNode OldRoot = Contract.AssertNotNull(await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false));
        SyntaxNode NewRoot = Contract.AssertNotNull(OldRoot.ReplaceNode(oldNode, newNode));

        // Return document with transformed tree.
        return document.WithSyntaxRoot(NewRoot);
    }
}
