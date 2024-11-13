namespace CSharpLatest;

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a code fix provider for CSL1006.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1006CodeFixProvider))]
[Shared]
public class CSL1006CodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Gets the list of fixable diagnostics.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds
    {
        get { return [CSL1006SimplifyOneLineSetter.DiagnosticId]; }
    }

    /// <summary>
    /// Gets the fix provider.
    /// </summary>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <summary>
    /// Registers a code action to invoke a fix.
    /// </summary>
    /// <param name="context">The code fix context.</param>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // Find the declaration identified by the diagnostic.
        var (Diagnostic, Declaration) = await CodeFixTools.FindNodeToFix<AccessorDeclarationSyntax>(context);

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.CSL1006CodeFixTitle,
                createChangedDocument: c => SimplifySetter(context.Document, Declaration, c),
                equivalenceKey: nameof(CodeFixResources.CSL1006CodeFixTitle)),
            Diagnostic);
    }

    private static async Task<Document> SimplifySetter(Document document,
        AccessorDeclarationSyntax accessorDeclaration,
        CancellationToken cancellationToken)
    {
        BlockSyntax Block = Contract.AssertNotNull(accessorDeclaration.Body);

        ExpressionStatementSyntax ExpressionStatement = (ExpressionStatementSyntax)Block.Statements[0];
        ExpressionSyntax Expression = ExpressionStatement.Expression;
        ArrowExpressionClauseSyntax ExpressionBody = SyntaxFactory.ArrowExpressionClause(Expression);
        SyntaxTriviaList PreservedAccessorDeclarationTrailingTrivia = accessorDeclaration.GetTrailingTrivia();

        AccessorDeclarationSyntax NewDeclaration = accessorDeclaration.WithBody(null)
                                                                        .WithExpressionBody(ExpressionBody)
                                                                        .WithSemicolonToken(ExpressionStatement.SemicolonToken)
                                                                        .WithTrailingTrivia(PreservedAccessorDeclarationTrailingTrivia);

        // Replace the old accessor with the new accessor.
        return await document.WithReplacedNode(accessorDeclaration, NewDeclaration, cancellationToken);
    }
}
