namespace CSharpLatest;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for rule CSL1010: 'init' accessor not supported in FieldBackedPropertyAttribute.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1010InitAccessorNotSupportedInFieldBackedPropertyAttribute : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1010";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1010AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1010AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1010AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId,
                                                            Title,
                                                            MessageFormat,
                                                            Category,
                                                            DiagnosticSeverity.Error,
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
        context = Contract.AssertNotNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.PropertyDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        AnalyzerTools.AssertSyntaxRequirements<PropertyDeclarationSyntax>(
            context,
            LanguageVersion.CSharp7,
            AnalyzeVerifiedNode,
            new SimpleAnalysisAssertion(context => PropertyGenerator.GetFirstSupportedAttribute(context, (PropertyDeclarationSyntax)context.Node) is not null));
    }

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, PropertyDeclarationSyntax propertyDeclaration, IEnumerable<IAnalysisAssertion> analysisAssertions)
    {
        // No diagnostic if there is no accessor list.
        if (propertyDeclaration.AccessorList is not AccessorListSyntax AccessorList)
            return;

        // No diagnostic if the accessor list doesn't contain 'init'.
        if (AccessorList.Accessors.FirstOrDefault(accessor => accessor.Keyword.IsKind(SyntaxKind.InitKeyword)) is not AccessorDeclarationSyntax AccessorDeclaration)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, AccessorDeclaration.GetLocation(), AccessorDeclaration.Keyword.Text));
    }
}
