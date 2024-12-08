namespace CSharpLatest;

using System;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Represents a simple analysis assertion.
/// </summary>
/// <param name="method">The assertion method.</param>
internal class SimpleAnalysisAssertion(Func<SyntaxNodeAnalysisContext, bool> method) : IAnalysisAssertion
{
    /// <inheritdoc />
    public bool IsTrue(SyntaxNodeAnalysisContext context) => Method(context);

    private readonly Func<SyntaxNodeAnalysisContext, bool> Method = method;
}
