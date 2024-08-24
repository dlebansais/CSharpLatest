namespace CSharpLatest;

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1001CodeFixProvider)), Shared]
public class CSL1001CodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds
    {
        get { return [CSL1001UseIsNull.DiagnosticId]; }
    }

    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // Find the expression identified by the diagnostic.
        var (Diagnostic, Expression) = await CodeFixTools.FindNodeToFix<BinaryExpressionSyntax>(context);

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.CSL1001CodeFixTitle,
                createChangedDocument: c => ChangeToIsNullAsync(context.Document, Expression, c),
                equivalenceKey: nameof(CodeFixResources.CSL1001CodeFixTitle)),
            Diagnostic);
    }

    private static async Task<Document> ChangeToIsNullAsync(Document document,
        BinaryExpressionSyntax binaryExpression,
        CancellationToken cancellationToken)
    {
        Document Result = document;

        // Remove the leading trivia from the expression operator.
        SyntaxToken OperatorToken = binaryExpression.OperatorToken;
        SyntaxTriviaList LeadingTrivia = OperatorToken.LeadingTrivia;

        // Save the trailing trivia in the expression right part.
        ExpressionSyntax RightExpression = binaryExpression.Right;
        SyntaxTriviaList TrailingTrivia = RightExpression.GetTrailingTrivia();

        // Produce the new expression.
        LiteralExpressionSyntax NullExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
        PatternSyntax NullPattern = SyntaxFactory.ConstantPattern(NullExpression);
        SyntaxToken IsToken = SyntaxFactory.Token(LeadingTrivia, SyntaxKind.IsKeyword, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));
        IsPatternExpressionSyntax NewExpression = SyntaxFactory.IsPatternExpression(binaryExpression.Left, IsToken, NullPattern);
        NewExpression = NewExpression.WithTrailingTrivia(TrailingTrivia);

        // Add an annotation to format the new local declaration.
        IsPatternExpressionSyntax FormattedExpression = NewExpression.WithAdditionalAnnotations(Formatter.Annotation);

        // Replace the old expression with the new expression.
        return await document.WithReplacedNode(cancellationToken, binaryExpression, FormattedExpression);
    }
}
