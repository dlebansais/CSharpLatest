namespace CSharpLatest;

using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Represents a type implementing analysis assertion.
/// </summary>
internal interface IAnalysisAssertion
{
    /// <summary>
    /// Checks whether the assertion is true for the provided context.
    /// </summary>
    /// <param name="context">The context.</param>
    bool IsTrue(SyntaxNodeAnalysisContext context);
}
