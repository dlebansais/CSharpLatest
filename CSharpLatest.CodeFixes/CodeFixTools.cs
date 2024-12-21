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

/// <summary>
/// Provides helpers related to fixing code.
/// </summary>
internal static class CodeFixTools
{
    /// <summary>
    /// Finds the node to fix.
    /// </summary>
    /// <typeparam name="T">The node type.</typeparam>
    /// <param name="context">The code fix context.</param>
    public static async Task<(Diagnostic Diagnostic, T Node)> FindNodeToFix<T>(CodeFixContext context)
        where T : CSharpSyntaxNode
    {
        SyntaxNode Root = Contract.AssertNotNull(await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false));

        Diagnostic Diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan DiagnosticSpan = Diagnostic.Location.SourceSpan;

        SyntaxNode Parent = Contract.AssertNotNull(Root.FindToken(DiagnosticSpan.Start).Parent);

        // Find the type declaration identified by the diagnostic.
        T Node = Contract.AssertNotNull(Parent.AncestorsAndSelf().OfType<T>().First());

        return (Diagnostic, Node);
    }

    /// <summary>
    /// Gets the semantic model of a document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public static async Task<SemanticModel> GetSemanticModel(Document document, CancellationToken cancellationToken)
    {
        SemanticModel SemanticModel = Contract.AssertNotNull(await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false));

        return SemanticModel;
    }

    /// <summary>
    /// Gets the converted type of a variable.
    /// </summary>
    /// <param name="semanticModel">The semantic model to use.</param>
    /// <param name="variableTypeName">The variable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public static ITypeSymbol GetVarConvertedType(SemanticModel semanticModel, TypeSyntax variableTypeName, CancellationToken cancellationToken)
    {
        ITypeSymbol VariableType = Contract.AssertNotNull(semanticModel.GetTypeInfo(variableTypeName, cancellationToken).ConvertedType);

        return VariableType;
    }

    /// <summary>
    /// Replaces a node in a document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="oldNode">The old node to replace.</param>
    /// <param name="newNode">The new node.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public static async Task<Document> WithReplacedNode(this Document document, SyntaxNode oldNode, SyntaxNode newNode, CancellationToken cancellationToken)
    {
        SyntaxNode OldRoot = Contract.AssertNotNull(await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false));
        SyntaxNode NewRoot = Contract.AssertNotNull(OldRoot.ReplaceNode(oldNode, newNode));

        // Return document with transformed tree.
        return document.WithSyntaxRoot(NewRoot);
    }
}
