namespace CSharpLatest;

using System.Collections.Immutable;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for rule CSL1005: Simplify one line getter.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1005SimplifyOneLineGetter : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1005";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1005AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1005AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1005AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
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

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.GetAccessorDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context) => AnalyzerTools.AssertSyntaxRequirements<AccessorDeclarationSyntax>(context, LanguageVersion.CSharp6, AnalyzeVerifiedNode);

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, AccessorDeclarationSyntax accessorDeclaration, IAnalysisAssertion[] analysisAssertions)
    {
        PropertyDeclarationSyntax PropertyDeclaration = Contract.AssertNotNull(accessorDeclaration.FirstAncestorOrSelf<PropertyDeclarationSyntax>());
        AccessorListSyntax AccessorList = Contract.AssertNotNull(PropertyDeclaration.AccessorList);
        bool IsSingleAccessor = AccessorList.Accessors.Count == 1;

        int StartLineNumber = -1;
        int EndLineNumber = 0;
        Location Location = accessorDeclaration.GetLocation();

        if (accessorDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ArrowExpressionClause)
        {
            // Ignore the property if there is more than just a getter, it cannot be simplified more.
            if (!IsSingleAccessor)
                return;

            FileLinePositionSpan Span = ArrowExpressionClause.SyntaxTree.GetLineSpan(ArrowExpressionClause.Span);
            StartLineNumber = Span.StartLinePosition.Line;
            EndLineNumber = Span.EndLinePosition.Line;
            Location = ArrowExpressionClause.GetLocation();
        }

        if (accessorDeclaration.Body is BlockSyntax Block)
        {
            CSharpParseOptions Options = (CSharpParseOptions)accessorDeclaration.SyntaxTree.Options;
            if (Options.LanguageVersion < LanguageVersion.CSharp7)
                return;

            // Ignore empty or multi-statements bodies.
            if (Block.Statements.Count != 1)
                return;

            // Ignore statements that are not return (like "throw").
            if (Block.Statements[0] is not ReturnStatementSyntax ReturnStatement)
                return;

            // Ignore empty return.
            if (ReturnStatement.Expression is null)
                return;

            FileLinePositionSpan Span = ReturnStatement.SyntaxTree.GetLineSpan(ReturnStatement.Span);
            StartLineNumber = Span.StartLinePosition.Line;
            EndLineNumber = Span.EndLinePosition.Line;
            Location = Block.GetLocation();
        }

        // Ignore multi-line statements.
        if (StartLineNumber < EndLineNumber)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, Location, PropertyDeclaration.Identifier.Text));
    }
}
