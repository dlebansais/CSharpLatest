namespace CSharpLatest;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

internal class DataFlowAnalysisAssertion<T> : IAnalysisAssertion
    where T : StatementSyntax
{
    public DataFlowAnalysisAssertion()
    {
    }

    public bool IsTrue(SyntaxNodeAnalysisContext context)
    {
        StatementSyntax Statement = (StatementSyntax)context.Node;
        DataFlowAnalysis? AnalysisResult = context.SemanticModel.AnalyzeDataFlow(Statement);
        bool AssertionResult = AnalysisResult is not null;
        DataFlowAnalysis = AnalysisResult!;

        return AssertionResult;
    }

    public DataFlowAnalysis DataFlowAnalysis { get; private set; } = null!;
}
