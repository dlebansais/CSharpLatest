namespace CSharpLatest;

using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

internal static class AnalyzerTools
{
    // Define this symbol in unit tests to simulate an assertion failure.
    // This will test branches that can only execute in future versions of C#.
    private const string CoverageDirectivePrefix = "#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952";

    public static string GetHelpLink(string diagnosticId)
    {
        return $"https://github.com/dlebansais/CSharpLatest/blob/master/doc/{diagnosticId}.md";
    }

    /// <summary>
    /// Asserts that the analyzed node is of the expected type and satisfies requirements, then executes <paramref name="continueAction"/>.
    /// </summary>
    /// <typeparam name="T">The type of the analyzed node.</typeparam>
    /// <param name="context">The analyzer context.</param>
    /// <param name="minimumLanguageVersion">The minimum language version supporting the feature.</param>
    /// <param name="continueAction">The next analysis step.</param>
    /// <param name="analysisAssertions">A list of assertions.</param>
    public static void AssertSyntaxRequirements<T>(SyntaxNodeAnalysisContext context, LanguageVersion minimumLanguageVersion, Action<SyntaxNodeAnalysisContext, T, IAnalysisAssertion[]> continueAction, params IAnalysisAssertion[] analysisAssertions)
        where T : CSharpSyntaxNode
    {
        T ValidNode = (T)context.Node;

        if (IsFeatureSupportedInThisVersion(context, minimumLanguageVersion))
        {
            bool IsCoverageContext = IsCalledForCoverage(context);
            bool AreAllAssertionsTrue = analysisAssertions.TrueForAll(context);

            if (!IsCoverageContext && AreAllAssertionsTrue)
                continueAction(context, ValidNode, analysisAssertions);
        }
    }

    private static bool IsFeatureSupportedInThisVersion(SyntaxNodeAnalysisContext context, LanguageVersion minimumLanguageVersion)
    {
        var ParseOptions = (CSharpParseOptions)context.SemanticModel.SyntaxTree.Options;
        return ParseOptions.LanguageVersion >= minimumLanguageVersion;
    }

    private static bool IsCalledForCoverage(SyntaxNodeAnalysisContext context)
    {
        string? FirstDirectiveText = context.SemanticModel.SyntaxTree.GetRoot().GetFirstDirective()?.GetText().ToString();
        return FirstDirectiveText is not null && FirstDirectiveText.StartsWith(CoverageDirectivePrefix);
    }

    private static bool TrueForAll(this IAnalysisAssertion[] analysisAssertions, SyntaxNodeAnalysisContext context)
    {
        return Array.TrueForAll(analysisAssertions, analysisAssertion => IsTrue(analysisAssertion, context));
    }

    private static bool IsTrue(this IAnalysisAssertion analysisAssertion, SyntaxNodeAnalysisContext context)
    {
        return analysisAssertion.IsTrue(context);
    }
}
