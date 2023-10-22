namespace CSharpLatest;

using System;
using Microsoft.CodeAnalysis.Diagnostics;

internal class SimpleAnalysisAssertion : IAnalysisAssertion
{
    public SimpleAnalysisAssertion(Func<SyntaxNodeAnalysisContext, bool> method)
    {
        Method = method;
    }

    public bool IsTrue(SyntaxNodeAnalysisContext context) => Method(context) == true;

    private Func<SyntaxNodeAnalysisContext, bool> Method;
}
