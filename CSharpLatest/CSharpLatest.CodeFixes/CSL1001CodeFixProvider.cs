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
using Microsoft.CodeAnalysis.Simplification;

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
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the expression identified by the diagnostic.
        var expression = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<BinaryExpressionSyntax>().First();

        if (expression is not null)
        {
            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CSL1001CodeFixTitle,
                    createChangedDocument: c => ChangeToIsNullAsync(context.Document, expression, c),
                    equivalenceKey: nameof(CodeFixResources.CSL1001CodeFixTitle)),
                diagnostic);
        }
    }

    private static async Task<Document> ChangeToIsNullAsync(Document document,
        BinaryExpressionSyntax binaryExpression,
        CancellationToken cancellationToken)
    {
        Document Result = document;

        // Remove the leading trivia from the expression.
        SyntaxToken firstToken = binaryExpression.GetFirstToken();
        SyntaxTriviaList leadingTrivia = firstToken.LeadingTrivia;
        BinaryExpressionSyntax trimmedExpression = binaryExpression.ReplaceToken(
            firstToken, firstToken.WithLeadingTrivia(SyntaxTriviaList.Empty));

        /*
        // Create a const token with the leading trivia.
        SyntaxToken constToken = SyntaxFactory.Token(leadingTrivia, SyntaxKind.ConstKeyword, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));

        // Insert the const token into the modifiers list, creating a new modifiers list.
        SyntaxTokenList newModifiers = trimmedExpression.Modifiers.Insert(0, constToken);


        // Produce the new expression.
        PatternSyntax pattern = SyntaxFactory.UnaryPattern()
        IsPatternExpressionSyntax newExpression = SyntaxFactory.IsPatternExpression(trimmedExpression.Left, );

        // Add an annotation to format the new local declaration.
        LocalDeclarationStatementSyntax formattedLocal = newExpression.WithAdditionalAnnotations(Formatter.Annotation);

        // Replace the old local declaration with the new local declaration.
        SyntaxNode? oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        SyntaxNode? newRoot = oldRoot?.ReplaceNode(binaryExpression, formattedLocal);

        // Return document with transformed tree.
        if (newRoot is not null)
            Result = document.WithSyntaxRoot(newRoot);
        */

        return Result;
    }
}
