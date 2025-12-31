namespace CSharpLatest;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Analyzer for rule CSL1014: Consider using inheritdoc.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1014ConsiderUsingInheritDoc : DiagnosticAnalyzer
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

    /// <summary>
    /// Initializes the rule analyzer.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context = Contract.AssertNotNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeEventField, SyntaxKind.EventFieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeEvent, SyntaxKind.EventDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeIndexer, SyntaxKind.IndexerDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context) => AnalyzerTools.AssertSyntaxRequirements<MethodDeclarationSyntax>(context, LanguageVersion.CSharp6, AnalyzeVerifiedMethod);

    private void AnalyzeEventField(SyntaxNodeAnalysisContext context) => AnalyzerTools.AssertSyntaxRequirements<EventFieldDeclarationSyntax>(context, LanguageVersion.CSharp6, AnalyzeVerifiedEventField);

    private void AnalyzeEvent(SyntaxNodeAnalysisContext context) => AnalyzerTools.AssertSyntaxRequirements<EventDeclarationSyntax>(context, LanguageVersion.CSharp6, AnalyzeVerifiedEvent);

    private void AnalyzeIndexer(SyntaxNodeAnalysisContext context) => AnalyzerTools.AssertSyntaxRequirements<IndexerDeclarationSyntax>(context, LanguageVersion.CSharp6, AnalyzeVerifiedIndexer);

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context) => AnalyzerTools.AssertSyntaxRequirements<PropertyDeclarationSyntax>(context, LanguageVersion.CSharp6, AnalyzeVerifiedProperty);

    private void AnalyzeVerifiedMethod(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodDeclaration, IEnumerable<IAnalysisAssertion> analysisAssertions)
    {
        IMethodSymbol MethodSymbol = Contract.AssertNotNull(context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken));
        AnalyzeVerifiedNode(context, methodDeclaration, MethodSymbol);
    }

    private void AnalyzeVerifiedEventField(SyntaxNodeAnalysisContext context, EventFieldDeclarationSyntax eventFieldDeclaration, IEnumerable<IAnalysisAssertion> analysisAssertions)
    {
        // Only support single event declarations.
        if (eventFieldDeclaration.Declaration.Variables.Count != 1)
            return;

        VariableDeclaratorSyntax VariableDeclarator = eventFieldDeclaration.Declaration.Variables.Single();
        IEventSymbol EventSymbol = (IEventSymbol)Contract.AssertNotNull(context.SemanticModel.GetDeclaredSymbol(VariableDeclarator, context.CancellationToken));

        AnalyzeVerifiedNode(context, eventFieldDeclaration, EventSymbol);
    }

    private void AnalyzeVerifiedEvent(SyntaxNodeAnalysisContext context, EventDeclarationSyntax eventDeclaration, IEnumerable<IAnalysisAssertion> analysisAssertions)
    {
        IEventSymbol EventSymbol = Contract.AssertNotNull(context.SemanticModel.GetDeclaredSymbol(eventDeclaration, context.CancellationToken));

        AnalyzeVerifiedNode(context, eventDeclaration, EventSymbol);
    }

    private void AnalyzeVerifiedIndexer(SyntaxNodeAnalysisContext context, IndexerDeclarationSyntax indexerDeclaration, IEnumerable<IAnalysisAssertion> analysisAssertions)
    {
        IPropertySymbol IndexerSymbol = Contract.AssertNotNull(context.SemanticModel.GetDeclaredSymbol(indexerDeclaration, context.CancellationToken));

        AnalyzeVerifiedNode(context, indexerDeclaration, IndexerSymbol);
    }

    private void AnalyzeVerifiedProperty(SyntaxNodeAnalysisContext context, PropertyDeclarationSyntax propertyDeclaration, IEnumerable<IAnalysisAssertion> analysisAssertions)
    {
        IPropertySymbol PropertySymbol = Contract.AssertNotNull(context.SemanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken));

        AnalyzeVerifiedNode(context, propertyDeclaration, PropertySymbol);
    }

    private static void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, MemberDeclarationSyntax memberDeclaration, ISymbol symbol)
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
