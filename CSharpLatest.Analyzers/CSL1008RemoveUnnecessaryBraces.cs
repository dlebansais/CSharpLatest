namespace CSharpLatest;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for rule CSL1008: Remove Unnecessary Braces.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1008RemoveUnnecessaryBraces : BraceDiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1008";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1008AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1008AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1008AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
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

    /// <inheritdoc />
    private protected override void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, CSharpSyntaxNode syntaxNode, string braceSettingValue, StatementSyntax embeddedStatement)
    {
        // Other cases are handled in CSL1007.
        if (braceSettingValue != BraceAnalysis.PreferBraceNever)
            return;

        // The embedded statement has braces, but has several statements or is the empty block.
        if (embeddedStatement is not BlockSyntax Block || Block.Statements.Count != 1)
            return;

        StatementSyntax SingleStatement = Block.Statements[0];

        // If the statement is multine, the rule should not apply.
        if (BraceAnalysis.IsConsideredMultiLineNoBrace(syntaxNode, Block, SingleStatement))
            return;

        // A 'if' with no 'else' must not conflict with the current 'if' with a 'else'.
        if (syntaxNode is IfStatementSyntax IfStatement &&
            IfStatement.Else is not null &&
            SingleStatement is IfStatementSyntax EmbeddedIfStatement &&
            EmbeddedIfStatement.Else is null)
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
