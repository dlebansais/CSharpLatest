namespace CSharpLatest;

using System;
using System.Collections.Generic;
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
/// Represents a code fix provider for CSL1007.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1007CodeFixProvider))]
[Shared]
public class CSL1007CodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Gets the list of fixable diagnostics.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [CSL1007AddMissingBraces.DiagnosticId];

    /// <summary>
    /// Gets the fix provider.
    /// </summary>
    public sealed override FixAllProvider? GetFixAllProvider() => null;

    /// <summary>
    /// Registers a code action to invoke a fix.
    /// </summary>
    /// <param name="context">The code fix context.</param>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // Find the declaration identified by the diagnostic.
        var (Diagnostic, Declaration) = await CodeFixTools.FindNodeToFix<CSharpSyntaxNode>(context);

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.CSL1007CodeFixTitle,
                createChangedDocument: c => AddBraces(context.Document, Declaration, c),
                equivalenceKey: nameof(CodeFixResources.CSL1007CodeFixTitle)),
            Diagnostic);
    }

    private static async Task<Document> AddBraces(Document document,
        CSharpSyntaxNode syntaxNode,
        CancellationToken cancellationToken)
    {
        SyntaxTriviaList PreservedAccessorDeclarationTrailingTrivia = syntaxNode.GetTrailingTrivia();
        CSharpSyntaxNode ReplacedNode = syntaxNode;
        BlockSyntax? NewBlock = null;

        if (syntaxNode is ElseClauseSyntax ElseClause)
        {
            ReplacedNode = ElseClause.Statement;
            NewBlock = SyntaxFactory.Block(ElseClause.Statement);
        }

        if (syntaxNode is StatementSyntax Statement)
        {
            Dictionary<Type, Func<CSharpSyntaxNode, StatementSyntax>> Table = new()
            {
                { typeof(DoStatementSyntax), (n) => ((DoStatementSyntax)n).Statement },
                { typeof(FixedStatementSyntax), (n) => ((FixedStatementSyntax)n).Statement },
                { typeof(CommonForEachStatementSyntax), (n) => ((CommonForEachStatementSyntax)n).Statement },
                { typeof(ForStatementSyntax), (n) => ((ForStatementSyntax)n).Statement },
                { typeof(IfStatementSyntax), (n) => ((IfStatementSyntax)n).Statement },
                { typeof(LockStatementSyntax), (n) => ((LockStatementSyntax)n).Statement },
                { typeof(UsingStatementSyntax), (n) => ((UsingStatementSyntax)n).Statement },
                { typeof(WhileStatementSyntax), (n) => ((WhileStatementSyntax)n).Statement },
            };

            foreach (KeyValuePair<Type, Func<CSharpSyntaxNode, StatementSyntax>> Entry in Table)
            {
                if (Entry.Key.IsAssignableFrom(syntaxNode.GetType()))
                {
                    StatementSyntax InnerStatement = Entry.Value(syntaxNode);

                    ReplacedNode = InnerStatement;
                    NewBlock = SyntaxFactory.Block(InnerStatement);
                    break;
                }
            }
        }

        NewBlock = Contract.AssertNotNull(NewBlock);
        NewBlock = NewBlock.WithTrailingTrivia(PreservedAccessorDeclarationTrailingTrivia);

        // Replace the old node with the new node.
        return await document.WithReplacedNode(ReplacedNode, NewBlock, cancellationToken);
    }
}
