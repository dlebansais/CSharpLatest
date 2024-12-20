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
/// Represents a code fix provider for CSL1008.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1008CodeFixProvider))]
[Shared]
public class CSL1008CodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Gets the list of fixable diagnostics.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [CSL1008RemoveUnnecessaryBraces.DiagnosticId];

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
        var (Diagnostic, Declaration) = await CodeFixTools.FindNodeToFix<CSharpSyntaxNode>(context).ConfigureAwait(false);

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.CSL1008CodeFixTitle,
                createChangedDocument: c => AddBraces(context.Document, Declaration, c),
                equivalenceKey: nameof(CodeFixResources.CSL1008CodeFixTitle)),
            Diagnostic);
    }

    private static async Task<Document> AddBraces(Document document,
        CSharpSyntaxNode syntaxNode,
        CancellationToken cancellationToken)
    {
        SyntaxTriviaList PreservedAccessorDeclarationTrailingTrivia = syntaxNode.GetTrailingTrivia();
        BlockSyntax? ReplacedBlock = null;

        if (syntaxNode is ElseClauseSyntax ElseClause)
        {
            ReplacedBlock = ElseClause.Statement as BlockSyntax;
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
                    ReplacedBlock = Entry.Value(syntaxNode) as BlockSyntax;
                    break;
                }
            }
        }

        ReplacedBlock = Contract.AssertNotNull(ReplacedBlock);
        Contract.Assert(ReplacedBlock.Statements.Count == 1);

        StatementSyntax InnerStatement = ReplacedBlock.Statements[0].WithoutTrailingTrivia();
        InnerStatement = InnerStatement.WithTrailingTrivia(PreservedAccessorDeclarationTrailingTrivia);

        // Replace the old node with the new node.
        return await document.WithReplacedNode(ReplacedBlock, InnerStatement, cancellationToken).ConfigureAwait(false);
    }
}
