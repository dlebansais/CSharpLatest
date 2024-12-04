namespace CSharpLatest;

using System.Collections.Immutable;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for rule CSL1008: Remove Unnecessary Braces.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1008RemoveUnnecessaryBraces : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1008";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1008AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1008AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1008AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
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

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        AnalyzerTools.AssertSyntaxRequirements<CSharpSyntaxNode>(context, AnalyzerTools.MinimumVersionAnalyzed, AnalyzeVerifiedNode);
    }

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, CSharpSyntaxNode syntaxNode, IAnalysisAssertion[] analysisAssertions)
    {
        string BraceSettingValue = AnalyzerTools.GetUserPreference(context, BraceAnalysis.PreferBraceSetting, BraceAnalysis.PreferBraceAlways);
        StatementSyntax EmbeddedStatement = BraceAnalysis.GetEmbeddedStatement(syntaxNode);

        // Other cases are handled in CSL1007.
        if (BraceSettingValue != BraceAnalysis.PreferBraceNever)
        {
            return;
        }

        if (EmbeddedStatement is not BlockSyntax Block || Block.Statements.Count != 1)
        {
            // The embedded statement has braces, but has several statements or is the empty block.
            return;
        }

        /*
        if (ContainsInterleavedDirective(syntaxNode, EmbeddedStatement, context.CancellationToken))
        {
            return;
        }*/

        SyntaxToken FirstToken = syntaxNode.GetFirstToken();
        context.ReportDiagnostic(Diagnostic.Create(Rule, FirstToken.GetLocation(), SyntaxFacts.GetText(FirstToken.Kind())));
    }
}
