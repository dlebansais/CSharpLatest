namespace CSharpLatest;

using System.Collections.Immutable;
using System.Linq;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Analyzer for rule CSL1014: Consider using inheritdoc.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1014ConsiderUsingInheritDoc : InheritDocDiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1014";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1014AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1014AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1014AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
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
        // Only analyze members that override a base member or implement an interface member.
        bool HasAncestor = symbol.IsOverride;
        bool HasInterface = ImplementsInterfaceMember(symbol);
        if (!HasAncestor && !HasInterface)
            return;

        // Ignore symbols without documentation comments.
        string? XmlDoc = symbol.GetDocumentationCommentXml(expandIncludes: false);
        if (string.IsNullOrEmpty(XmlDoc))
            return;

        XmlDoc = Contract.AssertNotNull(XmlDoc);

        // Ignore documentation already containing <inheritdoc />.
        if (XmlDoc.Contains("<inheritdoc"))
            return;

        // Gets the location of the documentation comment.
        SyntaxTriviaList xmlTrivia = memberDeclaration.GetLeadingTrivia();
        SyntaxTrivia First = TriviaToolsFirst.FirstTrivia(xmlTrivia);
        SyntaxTrivia Last = TriviaToolsLast.LastTrivia(xmlTrivia);

        // Adjust for trailing newlines in the last trivia.
        string LastTriviaString = Last.ToFullString();
        string TrimmedLastTriviaString = LastTriviaString.TrimEnd('\r', '\n');
        int TrimmedLength = LastTriviaString.Length - TrimmedLastTriviaString.Length;

        TextSpan DocSpan = TextSpan.FromBounds(First.FullSpan.Start, Last.Span.End - TrimmedLength);
        Location DocLocation = Location.Create(context.Node.SyntaxTree, DocSpan);
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
