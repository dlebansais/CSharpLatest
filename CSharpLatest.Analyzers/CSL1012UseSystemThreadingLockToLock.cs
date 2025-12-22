namespace CSharpLatest;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for rule CSL1012: Use System.Threading.Lock to lock.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1012UseSystemThreadingLockToLock : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1012";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1012AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1012AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1012AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
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

        context = Contract.AssertNotNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.LockStatement);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        AnalyzerTools.AssertSyntaxRequirements<LockStatementSyntax>(
            context,
            LanguageVersion.CSharp13,
            AnalyzeVerifiedNode);
    }

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, LockStatementSyntax lockStatement, IEnumerable<IAnalysisAssertion> analysisAssertions)
    {
        // Get the statement symbol.
        TypeInfo TypeInfo = context.SemanticModel.GetTypeInfo(lockStatement.Expression, context.CancellationToken);

        if (TypeInfo.Type is not INamedTypeSymbol LockExpressionTypeSymbol)
            return;

        if (context.Compilation.GetTypeByMetadataName("System.Threading.Lock") is not INamedTypeSymbol SystemThreadingLockSymbol)
            return;

        if (SymbolEqualityComparer.Default.Equals(LockExpressionTypeSymbol, SystemThreadingLockSymbol))
            return;

        // .NET older than 9 don't have System.Threading.Lock.
        if (!AnalyzerTools.IsDotNet(context.Compilation, minimumVersion: new Version(9, 0)))
            return;

        // Get the containing member (I could not figure out a way to get null here).
        MemberDeclarationSyntax MemberDeclaration = Contract.AssertNotNull(lockStatement.FirstAncestorOrSelf<MemberDeclarationSyntax>());

        // Get the member symbol.
        if (context.SemanticModel.GetDeclaredSymbol(MemberDeclaration, context.CancellationToken) is not ISymbol MemberSymbolInfo)
            return;

        // Ignore custom lock types declared in the same assembly.
        if (SymbolEqualityComparer.Default.Equals(MemberSymbolInfo.ContainingAssembly, LockExpressionTypeSymbol.ContainingAssembly))
            return;

        Location Location = lockStatement.Expression.GetLocation();

        context.ReportDiagnostic(Diagnostic.Create(Rule, Location, lockStatement.ToString()));
    }
}
