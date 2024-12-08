namespace CSharpLatest;

using System.Collections.Immutable;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for rule CSL1004: Consider using primary constructor.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1004ConsiderUsingRecord : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1004";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1004AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1004AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1004AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId,
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
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <summary>
    /// Initializes the rule analyzer.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    [Access("public", "override")]
    [RequireNotNull(nameof(context))]
    private void InitializeVerified(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context) => AnalyzerTools.AssertSyntaxRequirements<ClassDeclarationSyntax>(context, LanguageVersion.CSharp9, AnalyzeVerifiedNode);

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, IAnalysisAssertion[] analysisAssertions)
    {
        if (ConstructorAnalysis.GetBestSuggestion(classDeclaration) != ConstructorAnalysis.BestSuggestion.Record)
            return;

        // If the class inherits from a base, we can't change it to a record: no diagnostic.
        // BaseInfo.IsObject is false when we don't know what the base is.
        INamedTypeSymbol? TypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        BaseInfo BaseInfo = AnalyzerTools.GetBaseInfo(TypeSymbol);
        if (!BaseInfo.IsObject || BaseInfo.Depth > 1)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.Text));
    }
}
