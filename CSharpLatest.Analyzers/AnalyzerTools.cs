namespace CSharpLatest;

using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

internal static class AnalyzerTools
{
    // Define this symbol in unit tests to simulate an assertion failure.
    // This will test branches that can only execute in future versions of C#.
    private const string CoverageDirectivePrefix = "#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952";

    /// <summary>
    /// Asserts that the analyzed node is of the expected type and satisfies requirements, then executes <paramref name="continueAction"/>.
    /// </summary>
    /// <typeparam name="T">The type of the analyzed node.</typeparam>
    /// <param name="context">The analyzer context.</param>
    /// <param name="continueAction">The next analysis step.</param>
    /// <param name="assertions">A list of assertions.</param>
    public static void AssertSyntaxRequirements<T>(SyntaxNodeAnalysisContext context, Action<SyntaxNodeAnalysisContext, T> continueAction, params Func<SyntaxNodeAnalysisContext, bool>[] assertions)
        where T : CSharpSyntaxNode
    {
        var ValidNode = context.Node as T;

        string? FirstDirectiveText = context.SemanticModel.SyntaxTree.GetRoot().GetFirstDirective()?.GetText().ToString();
        bool IsCoverageContext = FirstDirectiveText is not null && FirstDirectiveText.StartsWith(CoverageDirectivePrefix);
        bool AreAllAssertionsTrue = assertions.TrueForAll(context);
        bool IsValid = !IsCoverageContext && AreAllAssertionsTrue;

        if (ValidNode is not null && IsValid)
            continueAction(context, ValidNode);
    }

    private static bool TrueForAll(this Func<SyntaxNodeAnalysisContext, bool>[] assertions, SyntaxNodeAnalysisContext context)
    {
        return Array.TrueForAll(assertions, assertion => assertion.IsTrue(context));
    }

    private static bool IsTrue(this Func<SyntaxNodeAnalysisContext, bool> assertion, SyntaxNodeAnalysisContext context)
    {
        return assertion(context) == true;
    }
}
