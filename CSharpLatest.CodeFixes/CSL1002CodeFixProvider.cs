namespace CSharpLatest;

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
using Microsoft.CodeAnalysis.Formatting;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1002CodeFixProvider)), Shared]
public class CSL1002CodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds
    {
        get { return ImmutableArray.Create(CSL1002UseIsNotNull.DiagnosticId); }
    }

    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var Root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var Diagnostic = context.Diagnostics.First();
        var DiagnosticSpan = Diagnostic.Location.SourceSpan;

        // Find the expression identified by the diagnostic.
        var Expression = Root?.FindToken(DiagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<BinaryExpressionSyntax>().First();

        if (Expression is not null)
        {
            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CSL1002CodeFixTitle,
                    createChangedDocument: c => ChangeToIsNotNullAsync(context.Document, Expression, c),
                    equivalenceKey: nameof(CodeFixResources.CSL1002CodeFixTitle)),
                Diagnostic);
        }
    }

    private static async Task<Document> ChangeToIsNotNullAsync(Document document,
        BinaryExpressionSyntax binaryExpression,
        CancellationToken cancellationToken)
    {
        Document Result = document;

        // Remove the leading trivia from the expression operator.
        SyntaxToken OperatorToken = binaryExpression.OperatorToken;
        SyntaxTriviaList LeadingTrivia = OperatorToken.LeadingTrivia;

        // Produce the new expression.
        LiteralExpressionSyntax NullExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
        PatternSyntax NullPattern = SyntaxFactory.ConstantPattern(NullExpression);
        SyntaxToken NotKeywordToken = SyntaxFactory.Token(SyntaxKind.NotKeyword);
        UnaryPatternSyntax NotNullPattern = SyntaxFactory.UnaryPattern(NotKeywordToken, NullPattern);
        SyntaxToken IsToken = SyntaxFactory.Token(LeadingTrivia, SyntaxKind.IsKeyword, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));
        IsPatternExpressionSyntax NewExpression = SyntaxFactory.IsPatternExpression(binaryExpression.Left, IsToken, NotNullPattern);

        // Add an annotation to format the new local declaration.
        IsPatternExpressionSyntax FormattedExpression = NewExpression.WithAdditionalAnnotations(Formatter.Annotation);

        // Replace the old expression with the new expression.
        SyntaxNode? OldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        SyntaxNode? NewRoot = OldRoot?.ReplaceNode(binaryExpression, FormattedExpression);

        // Return document with transformed tree.
        if (NewRoot is not null)
            Result = document.WithSyntaxRoot(NewRoot);

        return Result;
    }
}
