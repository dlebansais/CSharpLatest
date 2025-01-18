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
using Microsoft.CodeAnalysis.Formatting;

/// <summary>
/// Represents a code fix provider for CSL1002.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1002CodeFixProvider))]
[Shared]
public class CSL1002CodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Gets the list of fixable diagnostics.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [CSL1002UseIsNotNull.DiagnosticId];

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
        // Find the expression identified by the diagnostic.
        (Diagnostic Diagnostic, BinaryExpressionSyntax Expression) = await CodeFixTools.FindNodeToFix<BinaryExpressionSyntax>(context).ConfigureAwait(false);

        LocalizableString MessageFormat = CSL1002UseIsNotNull.MessageFormat;
        ExpressionSyntax RightExpression = Expression.Right;
        SyntaxToken OperatorToken = Expression.OperatorToken;
        string FormatParameter = $"{OperatorToken} {RightExpression}";
        string ExpectedDiagnosticMessage = string.Format(null, MessageFormat.ToString(null), FormatParameter);
        string Message = Diagnostic.GetMessage(null);
        Contract.Assert(ExpectedDiagnosticMessage == Message);

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.CSL1002CodeFixTitle,
                createChangedDocument: c => ChangeToIsNotNullAsync(context.Document, Expression, c),
                equivalenceKey: nameof(CodeFixResources.CSL1002CodeFixTitle)),
            Diagnostic);
    }

    private static async Task<Document> ChangeToIsNotNullAsync(Document document,
        BinaryExpressionSyntax binaryExpression,
        CancellationToken cancellationToken)
    {
        // Remove the leading trivia from the expression operator.
        SyntaxToken OperatorToken = binaryExpression.OperatorToken;
        SyntaxTriviaList LeadingTrivia = OperatorToken.LeadingTrivia;

        // Save the trailing trivia in the expression right part.
        ExpressionSyntax RightExpression = binaryExpression.Right;
        SyntaxTriviaList TrailingTrivia = RightExpression.GetTrailingTrivia();

        // Produce the new expression.
        LiteralExpressionSyntax NullExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
        PatternSyntax NullPattern = SyntaxFactory.ConstantPattern(NullExpression);
        SyntaxToken NotKeywordToken = SyntaxFactory.Token(SyntaxKind.NotKeyword);
        UnaryPatternSyntax NotNullPattern = SyntaxFactory.UnaryPattern(NotKeywordToken, NullPattern);
        SyntaxToken IsToken = SyntaxFactory.Token(LeadingTrivia, SyntaxKind.IsKeyword, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));
        IsPatternExpressionSyntax NewExpression = SyntaxFactory.IsPatternExpression(binaryExpression.Left, IsToken, NotNullPattern);
        NewExpression = NewExpression.WithTrailingTrivia(TrailingTrivia);

        // Add an annotation to format the new node.
        IsPatternExpressionSyntax FormattedExpression = NewExpression.WithAdditionalAnnotations(Formatter.Annotation);

        // Replace the old expression with the new expression.
        return await document.WithReplacedNode(binaryExpression, FormattedExpression, cancellationToken).ConfigureAwait(false);
    }
}
