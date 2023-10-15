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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1001CodeFixProvider)), Shared]
public class CSL1001CodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds
    {
        get { return ImmutableArray.Create(CSL1001Analyzer.DiagnosticId); }
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
                    title: CodeFixResources.CSL1001CodeFixTitle,
                    createChangedDocument: c => ChangeToIsNullAsync(context.Document, Expression, c),
                    equivalenceKey: nameof(CodeFixResources.CSL1001CodeFixTitle)),
                Diagnostic);
        }
    }

    private static async Task<Document> ChangeToIsNullAsync(Document document,
        BinaryExpressionSyntax binaryExpression,
        CancellationToken cancellationToken)
    {
        Document Result = document;

        // Remove the leading trivia from the expression operator.
        SyntaxToken OperatorToken = binaryExpression.OperatorToken;
        SyntaxTriviaList LeadingTrivia = OperatorToken.LeadingTrivia;

        // Produce the new expression.
        SyntaxToken IsToken = SyntaxFactory.Token(LeadingTrivia, SyntaxKind.IsKeyword, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));
        LiteralExpressionSyntax NullExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
        PatternSyntax Pattern = SyntaxFactory.ConstantPattern(NullExpression);
        IsPatternExpressionSyntax NewExpression = SyntaxFactory.IsPatternExpression(binaryExpression.Left, IsToken, Pattern);

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
