namespace CSharpLatest;

using System.Collections.Immutable;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for rule CSL1006: Simplify one line setter.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1006SimplifyOneLineSetter : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1006";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1006AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1006AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1006AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
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

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SetAccessorDeclaration, SyntaxKind.InitAccessorDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context) => AnalyzerTools.AssertSyntaxRequirements<AccessorDeclarationSyntax>(context, LanguageVersion.CSharp7, AnalyzeVerifiedNode);

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, AccessorDeclarationSyntax accessorDeclaration, IAnalysisAssertion[] analysisAssertions)
    {
        PropertyDeclarationSyntax PropertyDeclaration = Contract.AssertNotNull(accessorDeclaration.FirstAncestorOrSelf<PropertyDeclarationSyntax>());

        int StartLineNumber = -1;
        int EndLineNumber = 0;
        Location Location = accessorDeclaration.GetLocation();

        if (accessorDeclaration.Body is BlockSyntax Block)
        {
            // Ignore empty or multi-statements bodies.
            if (Block.Statements.Count != 1)
                return;

            // Ignore statements that are not some expression.
            if (Block.Statements[0] is not ExpressionStatementSyntax ExpressionStatement)
                return;

            FileLinePositionSpan Span = ExpressionStatement.SyntaxTree.GetLineSpan(ExpressionStatement.Span);
            StartLineNumber = Span.StartLinePosition.Line;
            EndLineNumber = Span.EndLinePosition.Line;
            Location = Block.GetLocation();
        }

        // Ignore the property if already simplified.
        if (accessorDeclaration.ExpressionBody is not null)
            return;

        // Ignore multi-line statements.
        if (StartLineNumber < EndLineNumber)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, Location, PropertyDeclaration.Identifier.Text));
    }
}
