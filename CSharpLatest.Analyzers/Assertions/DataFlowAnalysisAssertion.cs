namespace CSharpLatest;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Represents a dataflow assertion.
/// </summary>
/// <typeparam name="T">The type of statement with data flow.</typeparam>
internal class DataFlowAnalysisAssertion<T> : IAnalysisAssertion
    where T : StatementSyntax
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataFlowAnalysisAssertion{T}"/> class.
    /// </summary>
    public DataFlowAnalysisAssertion()
    {
    }

    /// <inheritdoc />
    public bool IsTrue(SyntaxNodeAnalysisContext context)
    {
        StatementSyntax Statement = (StatementSyntax)context.Node;
        DataFlowAnalysis = context.SemanticModel.AnalyzeDataFlow(Statement);

        return DataFlowAnalysis is not null;
    }

    /// <summary>
    /// Gets the data flow analysis if <see cref="IsTrue"/> returned <see langword="true"/>.
    /// </summary>
    public DataFlowAnalysis? DataFlowAnalysis { get; private set; }
}
