namespace CSharpLatest;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for rule CSL1007: Add Missing Braces.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1007AddMissingBraces : BraceDiagnosticAnalyzer
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

    /// <inheritdoc />
    private protected override void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, CSharpSyntaxNode syntaxNode, string braceSettingValue, StatementSyntax embeddedStatement)
    {
        SyntaxKind EmbeddedStatementKind = embeddedStatement.Kind();

        // PreferBraceNever is handled in CSL1008.
        if (braceSettingValue is BraceAnalysis.PreferBraceNone or BraceAnalysis.PreferBraceNever)
            return;

        // The embedded statement already has braces, which is always allowed.
        if (EmbeddedStatementKind is SyntaxKind.Block)
            return;

        // Constructs like the following are always allowed:
        //
        //   if (something)
        //   {
        //   }
        //   else if (somethingElse) // <-- 'if' nested in an 'else' clause
        //   {
        //   }
        if (EmbeddedStatementKind is SyntaxKind.IfStatement && syntaxNode is ElseClauseSyntax)
            return;

        // If we have something like this:
        //
        //    using (...)
        //    using (...)
        //    {
        //    }
        //
        // The first statement needs no block as it formatted with the same indentation.
        if (EmbeddedStatementKind is SyntaxKind.LockStatement or SyntaxKind.UsingStatement or SyntaxKind.FixedStatement && EmbeddedStatementKind == syntaxNode.Kind())
            return;

        SyntaxToken LastSignificantToken = GetLastSignificantToken(braceSettingValue, embeddedStatement);

        if ((braceSettingValue is BraceAnalysis.PreferBraceWhenMultiline or BraceAnalysis.PreferBraceRecursive)
            && !BraceAnalysis.IsConsideredMultiLine(syntaxNode, embeddedStatement, LastSignificantToken)
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

    private static SyntaxToken GetLastSignificantToken(string braceSettingValue, StatementSyntax embeddedStatement)
        => braceSettingValue is BraceAnalysis.PreferBraceRecursive
            ? BraceAnalysis.GetStatementLastSignificantToken(embeddedStatement)
            : embeddedStatement.GetLastToken();
}
