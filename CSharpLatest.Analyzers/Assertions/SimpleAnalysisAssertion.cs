namespace CSharpLatest;

using System;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Represents a simple analysis assertion.
/// </summary>
internal class SimpleAnalysisAssertion : IAnalysisAssertion
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleAnalysisAssertion"/> class.
    /// </summary>
    /// <param name="method">The assertion method.</param>
    public SimpleAnalysisAssertion(Func<SyntaxNodeAnalysisContext, bool> method)
    {
        Method = method;
    }

    /// <inheritdoc />
    public bool IsTrue(SyntaxNodeAnalysisContext context) => Method(context) == true;

    private readonly Func<SyntaxNodeAnalysisContext, bool> Method;
}
