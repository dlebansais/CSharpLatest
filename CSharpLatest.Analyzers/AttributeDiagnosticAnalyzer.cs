namespace CSharpLatest;

using System.Collections.Generic;
using Contracts;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for attribute rules.
/// </summary>
public abstract class AttributeDiagnosticAnalyzer : DiagnosticAnalyzer
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

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Attribute);
    }

    /// <summary>
    /// Analyzes the node for the attribute syntax.
    /// </summary>
    /// <param name="context">The context.</param>
    private protected abstract void AnalyzeNode(SyntaxNodeAnalysisContext context);

    /// <summary>
    /// Analyzes the syntax node after verifying that it meets the necessary requirements for analysis.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="analysisAssertions">The analysis assertions.</param>
    private protected abstract void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, AttributeSyntax attribute, IEnumerable<IAnalysisAssertion> analysisAssertions);
}
