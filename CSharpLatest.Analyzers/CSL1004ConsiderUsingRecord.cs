﻿namespace CSharpLatest;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for rule CSL1004: Consider using primary constructor.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CSL1004ConsiderUsingPrimaryConstructor : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1004";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1004AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1004AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1004AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,
                                                                                 Title,
                                                                                 MessageFormat,
                                                                                 Category,
                                                                                 DiagnosticSeverity.Warning,
                                                                                 isEnabledByDefault: true,
                                                                                 description: Description,
                                                                                 AnalyzerTools.GetHelpLink(DiagnosticId));

    /// <summary>
    /// Gets the list of supported diagnostic.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return [Rule]; } }

    /// <summary>
    /// Initializes the rule analyzer.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        AnalyzerTools.AssertSyntaxRequirements<ClassDeclarationSyntax>(context, LanguageVersion.CSharp9, AnalyzeVerifiedNode);
    }

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, IAnalysisAssertion[] analysisAssertions)
    {
        if (ConstructorAnalysis.GetBestSuggestion(classDeclaration) != ConstructorAnalysis.BestSuggestion.Record)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), classDeclaration.Identifier.Text));
    }
}
