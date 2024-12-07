namespace CSharpLatest;

using System.Collections.Immutable;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for rule CSL1007: Add Missing Braces.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1007AddMissingBraces : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1007";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1007AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1007AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1007AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private const string Category = "Style";

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
        SyntaxKind EmbeddedStatementKind = EmbeddedStatement.Kind();

        // PreferBraceNever is handled in CSL1008.
        if (BraceSettingValue is BraceAnalysis.PreferBraceNone or BraceAnalysis.PreferBraceNever)
        {
            return;
        }

        if (EmbeddedStatementKind is SyntaxKind.Block)
        {
            // The embedded statement already has braces, which is always allowed.
            return;
        }

        if (EmbeddedStatementKind is SyntaxKind.IfStatement && syntaxNode is ElseClauseSyntax)
        {
            // Constructs like the following are always allowed:
            //
            //   if (something)
            //   {
            //   }
            //   else if (somethingElse) // <-- 'if' nested in an 'else' clause
            //   {
            //   }
            return;
        }

        if (EmbeddedStatementKind is SyntaxKind.LockStatement or SyntaxKind.UsingStatement or SyntaxKind.FixedStatement &&
            EmbeddedStatementKind == syntaxNode.Kind())
        {
            // If we have something like this:
            //
            //    using (...)
            //    using (...)
            //    {
            //    }
            //
            // The first statement needs no block as it formatted with the same indentation.
            return;
        }

        SyntaxToken LastSignificantToken = BraceSettingValue is BraceAnalysis.PreferBraceRecursive
            ? BraceAnalysis.GetStatementLastSignificantToken(EmbeddedStatement)
            : EmbeddedStatement.GetLastToken();

        if ((BraceSettingValue is BraceAnalysis.PreferBraceWhenMultiline or BraceAnalysis.PreferBraceRecursive)
            && !BraceAnalysis.IsConsideredMultiLine(syntaxNode, EmbeddedStatement, LastSignificantToken)
            && !BraceAnalysis.RequiresBracesToMatchContext(syntaxNode))
        {
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
