namespace CSharpLatest;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for rule CSL1017: Consider using inheritdoc.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1017ConsiderUsingInheritDoc : InheritDocDiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1017";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1017AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1017AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1017AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
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
    private protected override void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, MemberDeclarationSyntax memberDeclaration, ISymbol symbol)
    {
        // Only analyze members that implement an interface member.
        bool HasInterface = ImplementsInterfaceMember(symbol);
        if (!HasInterface)
            return;

        if (!IsDocReplaceable(symbol))
            return;

        Location DocLocation = GetDocLocation(context, memberDeclaration);
        context.ReportDiagnostic(Diagnostic.Create(Rule, DocLocation, symbol.Name));
    }

    private static bool ImplementsInterfaceMember(ISymbol symbol)
    {
        INamedTypeSymbol containingType = symbol.ContainingType;
        var col = from INamedTypeSymbol CandidateInterface in containingType.AllInterfaces
                  from ISymbol Member in CandidateInterface.GetMembers()
                  let Implementation = containingType.FindImplementationForInterfaceMember(Member)
                  where SymbolEqualityComparer.Default.Equals(Implementation, symbol)
                  select new { };

        return col.Any();
    }
}
