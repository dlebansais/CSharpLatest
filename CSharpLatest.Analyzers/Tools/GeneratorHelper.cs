namespace CSharpLatest;

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Helper class for the code generator.
/// </summary>
internal static class GeneratorHelper
{
    private const string UsingDirectivePrefix = "using ";

    /// <summary>
    /// Checks whether using directives contain 'using global::System'.
    /// </summary>
    /// <param name="usings">The using directives to check.</param>
    /// <returns><see langword="true"/> if using directives contain 'using global::System'; otherwise, <see langword="false"/>.</returns>
    public static bool HasGlobalSystem(string usings)
    {
        if (usings.Length == 0)
            return false;

        string[] Lines = usings.Split('\n');

        foreach (string Line in Lines)
            if (Line == "using global::System;" || StringStartsWith(Line, "using global::System."))
                return true;

        return false;
    }

    /// <summary>
    /// Sorts using directives.
    /// </summary>
    /// <param name="usings">The using directives to sort.</param>
    /// <returns>The sorted directives.</returns>
    public static string SortUsings(string usings)
    {
        if (usings == string.Empty)
            return string.Empty;

        List<string> Namespaces = [];
        string[] Lines = usings.Split('\n');

        foreach (string Line in Lines)
            if (IsUsingDirective(Line, out string Directive))
                Namespaces.Add(Directive);

        Namespaces.Sort(SortWithSystemFirst);
        Namespaces = [.. Namespaces.Distinct()];

        string Result = string.Empty;

        foreach (string DirectiveNamespace in Namespaces)
            Result += $"\n{UsingDirectivePrefix}{DirectiveNamespace};";

        return Result + "\n";
    }

    private static bool IsUsingDirective(string line, out string directiveNamespace)
    {
        string TrimmedLine = line.Trim(' ').Trim('\n').Trim('\r');

        if (StringStartsWith(TrimmedLine, UsingDirectivePrefix))
        {
            string RawNamespace = TrimmedLine.Substring(UsingDirectivePrefix.Length, TrimmedLine.Length - UsingDirectivePrefix.Length - 1);
            string[] Names = RawNamespace.Split('.');

            List<string> TrimmedNames = [];
            foreach (string Name in Names)
                TrimmedNames.Add(Name.Trim());

            directiveNamespace = string.Join(".", TrimmedNames);
            return true;
        }

        Contract.Unused(out directiveNamespace);
        return false;
    }

    private static int SortWithSystemFirst(string line1, string line2)
    {
        if (IsSystemUsing(line1) && !IsSystemUsing(line2))
            return -1;
        else if (!IsSystemUsing(line1) && IsSystemUsing(line2))
            return 1;
        else
#if NETFRAMEWORK
            return string.Compare(line1, line2);
#else
            return string.Compare(line1, line2, StringComparison.Ordinal);
#endif
    }

    private static bool IsSystemUsing(string usingNamespace) => usingNamespace == "System" || StringStartsWith(usingNamespace, "System.") || usingNamespace == "global::System" || StringStartsWith(usingNamespace, "global::System.");

    /// <summary>
    /// Returns whether the string <paramref name="s"/> starts with the prefix <paramref name="prefix"/>, performing a <see cref="StringComparison.Ordinal"/> comparison.
    /// </summary>
    /// <param name="s">The string.</param>
    /// <param name="prefix">The prefix.</param>
    public static bool StringStartsWith(string s, string prefix) => s.StartsWith(prefix, StringComparison.Ordinal);

    /// <summary>
    /// Gets the updated documentation for a node.
    /// </summary>
    /// <param name="syntaxNode">The node.</param>
    /// <param name="documentation">The updated documentation upon return..</param>
    /// <returns><see langword="true"/> if the node has updated documentation; otherwise, <see langword="false"/>.</returns>
    public static bool HasUpdatedNodeDocumentation(SyntaxNode syntaxNode, [NotNullWhen(true)] out string? documentation)
    {
        if (syntaxNode.HasLeadingTrivia)
        {
            List<SyntaxTrivia> SupportedTrivias = GetSupportedTrivias(syntaxNode);

            // Trim consecutive end of lines until there is only at most one at the beginning.
            bool HadEndOfLine = false;
            while (HasStartingEndOfLineTrivias(SupportedTrivias))
            {
                int PreviousRemaining = SupportedTrivias.Count;

                HadEndOfLine = true;
                SupportedTrivias.RemoveAt(0);

                // Ensures that this while loop is not infinite.
                int Remaining = SupportedTrivias.Count;
                Contract.Assert(Remaining + 1 == PreviousRemaining);
            }

            if (HadEndOfLine)
            {
                // Trim whitespace trivias at start.
                while (IsFirstTriviaWhitespace(SupportedTrivias))
                {
                    int PreviousRemaining = SupportedTrivias.Count;

                    SupportedTrivias.RemoveAt(0);

                    // Ensures that this while loop is not infinite.
                    int Remaining = SupportedTrivias.Count;
                    Contract.Assert(Remaining + 1 == PreviousRemaining);
                }
            }

            // Remove successive whitespace trivias.
            int i = 0;
            while (i + 1 < SupportedTrivias.Count)
            {
                int PreviousRemaining = SupportedTrivias.Count - i;

                if (SupportedTrivias[i].IsKind(SyntaxKind.WhitespaceTrivia) && SupportedTrivias[i + 1].IsKind(SyntaxKind.WhitespaceTrivia))
                    SupportedTrivias.RemoveAt(i);
                else
                    i++;

                int Remaining = SupportedTrivias.Count - i;

                // Ensures that this while loop is not infinite.
                Contract.Assert(Remaining + 1 == PreviousRemaining);
            }

            SyntaxTriviaList LeadingTrivia = SyntaxFactory.TriviaList(SupportedTrivias);

            if (LeadingTrivia.Any(SyntaxKind.SingleLineDocumentationCommentTrivia))
            {
                documentation = LeadingTrivia.ToFullString().Trim('\r').Trim('\n').TrimEnd(' ');
                return true;
            }
        }

        documentation = null;
        return false;
    }

    /// <summary>
    /// Gets supported trivias of a node.
    /// </summary>
    /// <param name="syntaxNode">The node.</param>
    public static List<SyntaxTrivia> GetSupportedTrivias(SyntaxNode syntaxNode)
    {
        SyntaxTriviaList LeadingTrivia = syntaxNode.GetLeadingTrivia();

        List<SyntaxTrivia> SupportedTrivias = [];
        foreach (SyntaxTrivia trivia in LeadingTrivia)
            if (IsSupportedTrivia(trivia))
                SupportedTrivias.Add(trivia);

        return SupportedTrivias;
    }

    private static bool IsSupportedTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.EndOfLineTrivia) ||
               trivia.IsKind(SyntaxKind.WhitespaceTrivia) ||
               trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
               trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    private static bool HasStartingEndOfLineTrivias(List<SyntaxTrivia> trivias)
    {
        int Count = 0;

        for (int i = 0; i < trivias.Count; i++)
        {
            SyntaxTrivia Trivia = trivias[i];

            if (Trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                Count++;

                if (Count > 1)
                    return true;
            }
            else if (!Trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                return false;
            }
        }

        return false;
    }

    private static bool IsFirstTriviaWhitespace(List<SyntaxTrivia> trivias)
    {
        // If we reach this event there is at least one end of line, therefore at least one trivia.
        Contract.Assert(trivias.Count > 0);

        SyntaxTrivia FirstTrivia = trivias[0];

        return FirstTrivia.IsKind(SyntaxKind.WhitespaceTrivia);
    }

    /// <summary>
    /// Gets the leading trivia with a line end and a tab.
    /// </summary>
    /// <param name="tab">The tab to use.</param>
    public static SyntaxTriviaList GetLeadingTriviaWithLineEnd(string tab)
    {
        List<SyntaxTrivia> Trivias =
        [
            SyntaxFactory.EndOfLine("\n"),
            SyntaxFactory.Whitespace(tab),
        ];

        return SyntaxFactory.TriviaList(Trivias);
    }

    /// <summary>
    /// Gets the leading trivia without a line end and a tab.
    /// </summary>
    /// <param name="tab">The tab to use.</param>
    public static SyntaxTriviaList GetLeadingTriviaWithoutLineEnd(string tab)
    {
        List<SyntaxTrivia> Trivias =
        [
            SyntaxFactory.Whitespace(tab),
        ];

        return SyntaxFactory.TriviaList(Trivias);
    }

    /// <summary>
    /// Generates code attributes.
    /// </summary>
    public static SyntaxList<AttributeListSyntax> GenerateCodeAttributes()
    {
        NameSyntax AttributeName = SyntaxFactory.IdentifierName(AnalyzerTools.RemoveAttributeSuffix(nameof(GeneratedCodeAttribute)));

        string ToolName = GetToolName();
        SyntaxToken ToolNameToken = SyntaxFactory.Literal(ToolName);
        LiteralExpressionSyntax ToolNameExpression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, ToolNameToken);
        AttributeArgumentSyntax ToolNameAttributeArgument = SyntaxFactory.AttributeArgument(ToolNameExpression);

        string ToolVersion = GetToolVersion();
        SyntaxToken ToolVersionToken = SyntaxFactory.Literal(ToolVersion);
        LiteralExpressionSyntax ToolVersionExpression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, ToolVersionToken);
        AttributeArgumentSyntax ToolVersionAttributeArgument = SyntaxFactory.AttributeArgument(ToolVersionExpression);

        AttributeArgumentListSyntax ArgumentList = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([ToolNameAttributeArgument, ToolVersionAttributeArgument]));
        AttributeSyntax Attribute = SyntaxFactory.Attribute(AttributeName, ArgumentList);
        AttributeListSyntax AttributeList = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList([Attribute]));
        SyntaxList<AttributeListSyntax> Attributes = SyntaxFactory.List([AttributeList]);

        return Attributes;
    }

    /// <summary>
    /// Gets the executing assembly name.
    /// </summary>
    public static string GetToolName()
    {
        System.Reflection.AssemblyName ExecutingAssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        return ExecutingAssemblyName.Name.ToString();
    }

    /// <summary>
    /// Gets the executing assembly version.
    /// </summary>
    public static string GetToolVersion() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
}