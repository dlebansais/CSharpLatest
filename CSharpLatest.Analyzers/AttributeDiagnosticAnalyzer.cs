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

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        AnalyzerTools.AssertSyntaxRequirements<AttributeSyntax>(
            context,
            LanguageVersion.CSharp7,
            AnalyzeVerifiedNode,
            new SimpleAnalysisAssertion(context => IsPropertyAttribute(context, (AttributeSyntax)context.Node)));
    }

    private static bool IsPropertyAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attribute) => AnalyzerTools.IsExpectedAttribute(context, typeof(FieldBackedPropertyAttribute), attribute);

    /// <summary>
    /// Analyzes the syntax node after verifying that it meets the necessary requirements for analysis.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="analysisAssertions">The analysis assertions.</param>
    private protected abstract void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, AttributeSyntax attribute, IEnumerable<IAnalysisAssertion> analysisAssertions);
}
