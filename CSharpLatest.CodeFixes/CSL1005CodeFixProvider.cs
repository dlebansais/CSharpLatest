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
/// Represents a code fix provider for CSL1005.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1005CodeFixProvider))]
[Shared]
public class CSL1005CodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Gets the list of fixable diagnostics.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [CSL1005SimplifyOneLineGetter.DiagnosticId];

    /// <summary>
    /// Gets the fix provider.
    /// </summary>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <summary>
    /// Registers a code action to invoke a fix.
    /// </summary>
    /// <param name="context">The code fix context.</param>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // Find the declaration identified by the diagnostic.
        var (Diagnostic, Declaration) = await CodeFixTools.FindNodeToFix<AccessorDeclarationSyntax>(context).ConfigureAwait(false);

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.CSL1005CodeFixTitle,
                createChangedDocument: c => SimplifyGetter(context.Document, Declaration, c),
                equivalenceKey: nameof(CodeFixResources.CSL1005CodeFixTitle)),
            Diagnostic);
    }

    private static async Task<Document> SimplifyGetter(Document document,
        AccessorDeclarationSyntax accessorDeclaration,
        CancellationToken cancellationToken)
    {
        Document Result = document;

        PropertyDeclarationSyntax PropertyDeclaration = Contract.AssertNotNull(accessorDeclaration.FirstAncestorOrSelf<PropertyDeclarationSyntax>());
        AccessorListSyntax AccessorList = Contract.AssertNotNull(PropertyDeclaration.AccessorList);
        bool IsSingleAccessor = AccessorList.Accessors.Count == 1;

        if (accessorDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ArrowExpressionClause)
        {
            ArrowExpressionClauseSyntax ExpressionBody = SyntaxFactory.ArrowExpressionClause(ArrowExpressionClause.Expression);

            SyntaxTriviaList PreservedPropertyDeclarationTrailingTrivia = PropertyDeclaration.GetTrailingTrivia();

            PropertyDeclarationSyntax NewPropertyDeclaration = PropertyDeclaration.WithAccessorList(null)
                                                                                  .WithExpressionBody(ExpressionBody)
                                                                                  .WithSemicolonToken(accessorDeclaration.SemicolonToken)
                                                                                  .WithTrailingTrivia(PreservedPropertyDeclarationTrailingTrivia);

            // Replace the old property with the new property.
            Result = await document.WithReplacedNode(PropertyDeclaration, NewPropertyDeclaration, cancellationToken).ConfigureAwait(false);
        }

        if (accessorDeclaration.Body is BlockSyntax Block)
        {
            ReturnStatementSyntax ReturnStatement = (ReturnStatementSyntax)Block.Statements[0];
            ExpressionSyntax Expression = Contract.AssertNotNull(ReturnStatement.Expression);
            ArrowExpressionClauseSyntax ExpressionBody = SyntaxFactory.ArrowExpressionClause(Expression);

            if (IsSingleAccessor)
            {
                SyntaxTriviaList PreservedPropertyDeclarationTrailingTrivia = PropertyDeclaration.GetTrailingTrivia();

                PropertyDeclarationSyntax NewPropertyDeclaration = PropertyDeclaration.WithAccessorList(null)
                                                                                      .WithExpressionBody(ExpressionBody)
                                                                                      .WithSemicolonToken(ReturnStatement.SemicolonToken)
                                                                                      .WithTrailingTrivia(PreservedPropertyDeclarationTrailingTrivia);

                // Replace the old property with the new property.
                Result = await document.WithReplacedNode(PropertyDeclaration, NewPropertyDeclaration, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                SyntaxTriviaList PreservedAccessorDeclarationTrailingTrivia = accessorDeclaration.GetTrailingTrivia();

                AccessorDeclarationSyntax NewDeclaration = accessorDeclaration.WithBody(null)
                                                                              .WithExpressionBody(ExpressionBody)
                                                                              .WithSemicolonToken(ReturnStatement.SemicolonToken)
                                                                              .WithTrailingTrivia(PreservedAccessorDeclarationTrailingTrivia);

                // Replace the old accessor with the new accessor.
                Result = await document.WithReplacedNode(accessorDeclaration, NewDeclaration, cancellationToken).ConfigureAwait(false);
            }
        }

        return Result;
    }
}
