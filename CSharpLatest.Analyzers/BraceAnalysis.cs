﻿namespace CSharpLatest;

using System;
using System.Collections.Generic;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Provides helpers for analyzing constructors.
/// </summary>
public static partial class BraceAnalysis
{
    /// <summary>
    /// The 'prefer brace' setting.
    /// </summary>
    public const string PreferBraceSetting = "csharp_prefer_braces";

    /// <summary>
    /// The 'true' value for <see cref="PreferBraceSetting"/>.
    /// </summary>
    public const string PreferBraceAlways = "true";

    /// <summary>
    /// The 'none' value for <see cref="PreferBraceSetting"/>.
    /// </summary>
    public const string PreferBraceNone = "false";

    /// <summary>
    /// The 'when_multiline' value for <see cref="PreferBraceSetting"/>.
    /// </summary>
    public const string PreferBraceWhenMultiline = "when_multiline";

    /// <summary>
    /// The 'never' value for <see cref="PreferBraceSetting"/>.
    /// </summary>
    public const string PreferBraceNever = "never";

    /// <summary>
    /// The 'recursive' value for <see cref="PreferBraceSetting"/>.
    /// </summary>
    public const string PreferBraceRecursive = "recursive";

    /// <summary>
    /// Gets the embedded statement of a if, else, do ... statement.
    /// </summary>
    /// <param name="syntaxNode">The syntax node with the embedded statement.</param>
    [RequireNotNull(nameof(syntaxNode))]
    private static StatementSyntax GetEmbeddedStatementVerified(CSharpSyntaxNode syntaxNode)
    {
        Dictionary<Type, Func<CSharpSyntaxNode, StatementSyntax>> Table = new()
        {
            { typeof(DoStatementSyntax), (n) => ((DoStatementSyntax)n).Statement },
            { typeof(ElseClauseSyntax), (n) => ((ElseClauseSyntax)n).Statement },
            { typeof(FixedStatementSyntax), (n) => ((FixedStatementSyntax)n).Statement },
            { typeof(CommonForEachStatementSyntax), (n) => ((CommonForEachStatementSyntax)n).Statement },
            { typeof(ForStatementSyntax), (n) => ((ForStatementSyntax)n).Statement },
            { typeof(IfStatementSyntax), (n) => ((IfStatementSyntax)n).Statement },
            { typeof(LockStatementSyntax), (n) => ((LockStatementSyntax)n).Statement },
            { typeof(UsingStatementSyntax), (n) => ((UsingStatementSyntax)n).Statement },
            { typeof(WhileStatementSyntax), (n) => ((WhileStatementSyntax)n).Statement },
        };

        StatementSyntax? Result = null;

        foreach (KeyValuePair<Type, Func<CSharpSyntaxNode, StatementSyntax>> Entry in Table)
        {
            if (Entry.Key.IsAssignableFrom(syntaxNode.GetType()))
            {
                Result = Entry.Value(syntaxNode);
                break;
            }
        }

        return Contract.AssertNotNull(Result);
    }

    /// <summary>
    /// Gets the last token of a statement for if, do ... statements.
    /// </summary>
    /// <param name="statement">The statement.</param>
    [RequireNotNull(nameof(statement))]
    private static SyntaxToken GetStatementLastSignificantTokenVerified(StatementSyntax statement)
    {
        Dictionary<Type, Func<CSharpSyntaxNode, SyntaxToken>> Table = new()
        {
            { typeof(DoStatementSyntax), (n) => ((DoStatementSyntax)n).DoKeyword },
            { typeof(FixedStatementSyntax), (n) => ((FixedStatementSyntax)n).CloseParenToken },
            { typeof(CommonForEachStatementSyntax), (n) => ((CommonForEachStatementSyntax)n).CloseParenToken },
            { typeof(ForStatementSyntax), (n) => ((ForStatementSyntax)n).CloseParenToken },
            { typeof(IfStatementSyntax), (n) => ((IfStatementSyntax)n).CloseParenToken },
            { typeof(LockStatementSyntax), (n) => ((LockStatementSyntax)n).CloseParenToken },
            { typeof(UsingStatementSyntax), (n) => ((UsingStatementSyntax)n).CloseParenToken },
            { typeof(WhileStatementSyntax), (n) => ((WhileStatementSyntax)n).CloseParenToken },
        };

        SyntaxToken Result = statement.GetLastToken();

        foreach (KeyValuePair<Type, Func<CSharpSyntaxNode, SyntaxToken>> Entry in Table)
        {
            if (Entry.Key.IsAssignableFrom(statement.GetType()))
            {
                Result = Entry.Value(statement);
                break;
            }
        }

        return Result;
    }

    /// <summary>
    /// Checks whether a node embeds a multiline node.
    /// </summary>
    /// <param name="syntaxNode">The syntax node.</param>
    /// <param name="embeddedStatement">The embedded statement.</param>
    /// <param name="lastSignificantToken">The last significant token in <paramref name="embeddedStatement"/>.</param>
    [RequireNotNull(nameof(syntaxNode))]
    [RequireNotNull(nameof(embeddedStatement))]
    private static bool IsConsideredMultiLineVerified(SyntaxNode syntaxNode, StatementSyntax embeddedStatement, SyntaxToken lastSignificantToken)
    {
        // Early return if syntax errors prevent analysis.
        if (embeddedStatement.IsMissing)
        {
            // The embedded statement was added by the compiler during recovery from a syntax error.
            return false;
        }

        // Early return if the entire statement fits on one line.
        if (AreTwoTokensOnSameLine(syntaxNode.GetFirstToken(), syntaxNode.GetLastToken()))
        {
            // The entire statement fits on one line. Examples:
            //
            //   if (something) return;
            //
            //   while (true) something();
            return false;
        }

        // Check the part of the statement preceding the embedded statement (bullet 1).
        SyntaxToken lastTokenBeforeEmbeddedStatement = embeddedStatement.GetFirstToken().GetPreviousToken();
        if (!AreTwoTokensOnSameLine(syntaxNode.GetFirstToken(), lastTokenBeforeEmbeddedStatement))
        {
            // The part of the statement preceding the embedded statement does not fit on one line. Examples:
            //
            //   for (int i = 0; // <-- The initializer/condition/increment are on separate lines
            //        i < 10;
            //        i++)
            //     SomeMethod();
            return true;
        }

        // Check the embedded statement itself (bullet 2).
        if (!AreTwoTokensOnSameLine(embeddedStatement.GetFirstToken(), lastSignificantToken))
        {
            // The embedded statement does not fit on one line. Examples:
            //
            //   if (something)
            //     obj.Method(   // <-- This embedded statement spans two lines.
            //       arg);
            return true;
        }

        // Check the part of the statement following the embedded statement, but only if it exists and is not an 'else' clause (bullet 3).
        if (syntaxNode.GetLastToken() != embeddedStatement.GetLastToken())
        {
            if (syntaxNode is IfStatementSyntax ifStatement && ifStatement.Statement == embeddedStatement)
            {
                // The embedded statement is followed by an 'else' clause, which may span multiple lines without
                // triggering a braces requirement, such as this:
                //
                //   if (true)
                //     return;
                //   else          // <-- this else clause is two lines, but is not considered a multiline context
                //     return;
                //
                // ---
                // INTENTIONAL FALLTHROUGH
            }
            else
            {
                SyntaxToken firstTokenAfterEmbeddedStatement = embeddedStatement.GetLastToken().GetNextToken();
                if (!AreTwoTokensOnSameLine(firstTokenAfterEmbeddedStatement, syntaxNode.GetLastToken()))
                {
                    // The part of the statement following the embedded statement does not fit on one line. Examples:
                    //
                    //   do
                    //     SomeMethod();
                    //   while (x < 0 ||    // <-- This condition is split across multiple lines.
                    //          x > 10);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether a syntax node requires braces.
    /// </summary>
    /// <param name="syntaxNode">The syntax node.</param>
    [RequireNotNull(nameof(syntaxNode))]
    private static bool RequiresBracesToMatchContextVerified(SyntaxNode syntaxNode)
    {
        if (syntaxNode.Kind() is not (SyntaxKind.IfStatement or SyntaxKind.ElseClause))
        {
            // 'if' statements are the only statements that can have multiple embedded statements which are
            // considered relative to each other.
            return false;
        }

        IfStatementSyntax outermostIfStatement = GetOutermostIfStatementOfSequence(syntaxNode);
        return AnyPartOfIfSequenceUsesBraces(outermostIfStatement);
    }

    private static bool AreTwoTokensOnSameLine(SyntaxToken token1, SyntaxToken token2)
    {
        if (token1 == token2)
        {
            return true;
        }

        SyntaxTree SyntaxTree = Contract.AssertNotNull(token1.SyntaxTree);
        SourceText Text = SyntaxTree.GetText();
        return AreOnSameLine(Text, token1, token2);
    }

    private static bool AreOnSameLine(SourceText text, SyntaxToken token1, SyntaxToken token2)
    {
        return AreOnSameLine(text, token1.Span.End, token2.SpanStart);
    }

    private static bool AreOnSameLine(SourceText text, int pos1, int pos2)
    {
        return text.Lines.IndexOf(pos1) == text.Lines.IndexOf(pos2);
    }

    private static IfStatementSyntax GetOutermostIfStatementOfSequence(SyntaxNode ifStatementOrElseClause)
    {
        IfStatementSyntax result;
        if (ifStatementOrElseClause.IsKind(SyntaxKind.ElseClause))
        {
            result = (IfStatementSyntax)GetRequiredParent(ifStatementOrElseClause);
        }
        else
        {
            // Debug.Assert(ifStatementOrElseClause.IsKind(SyntaxKind.IfStatement));
            result = (IfStatementSyntax)ifStatementOrElseClause;
        }

        while (result.Parent.IsKind(SyntaxKind.ElseClause))
            result = (IfStatementSyntax)GetRequiredParent(GetRequiredParent(result));

        return result;
    }

    private static SyntaxNode GetRequiredParent(SyntaxNode node)
    {
        return Contract.AssertNotNull(node.Parent);
    }

    private static bool AnyPartOfIfSequenceUsesBraces(IfStatementSyntax? statement)
    {
        // Iterative instead of recursive to avoid stack depth problems
        while (statement is not null)
        {
            if (statement.Statement.IsKind(SyntaxKind.Block))
                return true;

            StatementSyntax? elseStatement = statement.Else?.Statement;
            if (elseStatement.IsKind(SyntaxKind.Block))
                return true;

            statement = elseStatement as IfStatementSyntax;
        }

        return false;
    }
}