namespace CSharpLatest;

using System;
using System.Collections.Generic;
using System.Text;
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
    /// Checks whether a node embeds a multiline statement.
    /// </summary>
    /// <param name="syntaxNode">The syntax node.</param>
    /// <param name="embeddedStatement">The embedded statement.</param>
    [RequireNotNull(nameof(syntaxNode))]
    [RequireNotNull(nameof(embeddedStatement))]
    private static bool IsConsideredMultiLineVerified(SyntaxNode syntaxNode, StatementSyntax embeddedStatement)
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
        if (!AreTwoTokensOnSameLine(embeddedStatement.GetFirstToken(), embeddedStatement.GetLastToken()))
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

        SyntaxTree? tree = token1.SyntaxTree;
        return tree is not null && tree.TryGetText(out SourceText? text)
            ? AreOnSameLine(text, token1, token2)
            : !ContainsLineBreak(GetTextBetween(token1, token2));
    }

    private static bool AreOnSameLine(SourceText text, SyntaxToken token1, SyntaxToken token2)
    {
        return token1.RawKind != 0 &&
           token2.RawKind != 0 &&
           AreOnSameLine(text, token1.Span.End, token2.SpanStart);
    }

    private static bool AreOnSameLine(SourceText text, int pos1, int pos2)
    {
        return text.Lines.IndexOf(pos1) == text.Lines.IndexOf(pos2);
    }

    private static bool ContainsLineBreak(string text)
    {
        foreach (char ch in text)
        {
            if (ch is '\n' or '\r')
            {
                return true;
            }
        }

        return false;
    }

    private static string GetTextBetween(SyntaxToken token1, SyntaxToken token2)
    {
        StringBuilder builder = new();
        AppendTextBetween(token1, token2, builder);

        return builder.ToString();
    }

    private static void AppendTextBetween(SyntaxToken token1, SyntaxToken token2, StringBuilder builder)
    {
        // Contract.ThrowIfTrue(token1.RawKind == 0 && token2.RawKind == 0);
        // Contract.ThrowIfTrue(token1.Equals(token2));
        if (token1.RawKind == 0)
        {
            AppendLeadingTriviaText(token2, builder);
            return;
        }

        if (token2.RawKind == 0)
        {
            AppendTrailingTriviaText(token1, builder);
            return;
        }

        if (token1.FullSpan.End == token2.FullSpan.Start)
        {
            AppendTextBetweenTwoAdjacentTokens(token1, token2, builder);
            return;
        }

        AppendTrailingTriviaText(token1, builder);

        for (SyntaxToken token = token1.GetNextToken(includeZeroWidth: true); token.FullSpan.End <= token2.FullSpan.Start; token = token.GetNextToken(includeZeroWidth: true))
        {
            _ = builder.Append(token.ToFullString());
        }

        AppendPartialLeadingTriviaText(token2, builder, token1.TrailingTrivia.FullSpan.End);
    }

    private static void AppendTextBetweenTwoAdjacentTokens(SyntaxToken token1, SyntaxToken token2, StringBuilder builder)
    {
        AppendTrailingTriviaText(token1, builder);
        AppendLeadingTriviaText(token2, builder);
    }

    /// <summary>
    /// If the token1 is expected to be part of the leading trivia of the token2 then the trivia
    /// before the token1FullSpanEnd, which the fullspan end of the token1 should be ignored.
    /// </summary>
    private static void AppendPartialLeadingTriviaText(SyntaxToken token, StringBuilder builder, int token1FullSpanEnd)
    {
        if (!token.HasLeadingTrivia)
        {
            return;
        }

        foreach (SyntaxTrivia trivia in token.LeadingTrivia)
        {
            if (trivia.FullSpan.End <= token1FullSpanEnd)
            {
                continue;
            }

            _ = builder.Append(trivia.ToFullString());
        }
    }

    private static void AppendLeadingTriviaText(SyntaxToken token, StringBuilder builder)
    {
        if (!token.HasLeadingTrivia)
        {
            return;
        }

        foreach (SyntaxTrivia trivia in token.LeadingTrivia)
        {
            _ = builder.Append(trivia.ToFullString());
        }
    }

    private static void AppendTrailingTriviaText(SyntaxToken token, StringBuilder builder)
    {
        if (!token.HasTrailingTrivia)
        {
            return;
        }

        foreach (SyntaxTrivia trivia in token.TrailingTrivia)
        {
            _ = builder.Append(trivia.ToFullString());
        }
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
        return node.Parent ?? throw new InvalidOperationException("Token's parent was null");
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
