namespace CSharpLatest;

using System.Collections.Generic;
using Contracts;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for brace rules.
/// </summary>
public abstract class BraceDiagnosticAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Initializes the rule analyzer.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context = Contract.AssertNotNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode,
                                         SyntaxKind.DoStatement,
                                         SyntaxKind.ElseClause,
                                         SyntaxKind.FixedStatement,
                                         SyntaxKind.ForEachStatement,
                                         SyntaxKind.ForEachVariableStatement,
                                         SyntaxKind.ForStatement,
                                         SyntaxKind.IfStatement,
                                         SyntaxKind.LockStatement,
                                         SyntaxKind.UsingStatement,
                                         SyntaxKind.WhileStatement);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context) => AnalyzerTools.AssertSyntaxRequirements<CSharpSyntaxNode>(context, AnalyzerTools.MinimumVersionAnalyzed, AnalyzeVerifiedNode);

    /// <summary>
    /// Analyzes the syntax node after verifying that it meets the necessary requirements for analysis.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="syntaxNode">The syntax node.</param>
    /// <param name="analysisAssertions">The analysis assertions.</param>
    private protected abstract void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, CSharpSyntaxNode syntaxNode, IEnumerable<IAnalysisAssertion> analysisAssertions);
}
