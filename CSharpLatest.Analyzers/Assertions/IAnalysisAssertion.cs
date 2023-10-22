namespace CSharpLatest;

using Microsoft.CodeAnalysis.Diagnostics;

internal interface IAnalysisAssertion
{
    bool IsTrue(SyntaxNodeAnalysisContext context);
}
