namespace CSharpLatest;

using System.Collections.Generic;
using System.Collections.Immutable;
#if ENABLE_CSL1013
using System.Linq;
#endif
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for rule CSL1013: Change extension function to extension member.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1013ChangeExtensionFunctionToExtensionMember : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1013";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1013AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1013AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1013AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
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
    public override void Initialize(AnalysisContext context)
    {
        context = Contract.AssertNotNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
    }

#if ENABLE_CSL1013
    private void AnalyzeNode(SyntaxNodeAnalysisContext context) => AnalyzerTools.AssertSyntaxRequirements<MethodDeclarationSyntax>(context, LanguageVersion.CSharp14, AnalyzeVerifiedNode);

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodDeclaration, IEnumerable<IAnalysisAssertion> analysisAssertions)
    {
        SeparatedSyntaxList<ParameterSyntax> Parameters = methodDeclaration.ParameterList.Parameters;
        SyntaxTokenList Modifiers = methodDeclaration.Modifiers;

        // The method must be static.
        if (!Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.StaticKeyword)))
            return;

        // The method must have at least one parameter.
        if (Parameters.Count < 1)
            return;

        ParameterSyntax FirstParameter = Parameters[0];
        SyntaxTokenList FirstParameterModifiers = FirstParameter.Modifiers;

        // The first parameter must have the 'this' modifier.
        if (FirstParameterModifiers.Count != 1)
            return;
        if (!FirstParameterModifiers[0].IsKind(SyntaxKind.ThisKeyword))
            return;

        Location Location = methodDeclaration.GetLocation();

        context.ReportDiagnostic(Diagnostic.Create(Rule, Location, methodDeclaration.Identifier.Text));
    }
#else
    private void AnalyzeNode(SyntaxNodeAnalysisContext context) => AnalyzerTools.AssertSyntaxRequirements<MethodDeclarationSyntax>(context, LanguageVersion.Default, AnalyzeVerifiedNode);

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodDeclaration, IEnumerable<IAnalysisAssertion> analysisAssertions)
    {
        // Never emit diagnostic.
    }
#endif
}
