namespace CSharpLatest;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynHelpers;

/// <summary>
/// Analyzer for rule CSL1016: Unsupported use of the AsyncEventHandler attribute.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1016UnsupportedUseOfTheAsyncEventHandlerAttribute : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1016";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1016AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1016AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1016AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId,
                                                            Title,
                                                            MessageFormat,
                                                            Category,
                                                            DiagnosticSeverity.Error,
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

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Attribute);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        AnalyzerTools.AssertSyntaxRequirements<AttributeSyntax>(
            context,
            LanguageVersion.CSharp7,
            AnalyzeVerifiedNode,
            new SimpleAnalysisAssertion(context => IsHandlerAttribute(context, (AttributeSyntax)context.Node)));
    }

    private static bool IsHandlerAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attribute) => AnalyzerTools.IsExpectedAttribute(context, typeof(AsyncEventHandlerAttribute), attribute);

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, AttributeSyntax attribute, IEnumerable<IAnalysisAssertion> analysisAssertions)
    {
        // Diagnostic unless for a method.
        if (attribute.FirstAncestorOrSelf<MethodDeclarationSyntax>() is MethodDeclarationSyntax MethodDeclaration)
        {
            if (IsValidEnvironment(MethodDeclaration) &&
                IsValidSignature(MethodDeclaration) &&
                IsValidArguments(attribute))
                return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), AttributeHelper.ToAttributeName(attribute)));
    }

    // No diagnostic if there is no class/record/struct or namespace.
    private static bool IsValidEnvironment(MethodDeclarationSyntax methodDeclaration)
        => HasClassAncestor(methodDeclaration) && HasNamespaceAncestor(methodDeclaration);

    private static bool HasClassAncestor(MethodDeclarationSyntax methodDeclaration)
        => methodDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>() is not null ||
           methodDeclaration.FirstAncestorOrSelf<StructDeclarationSyntax>() is not null ||
           methodDeclaration.FirstAncestorOrSelf<RecordDeclarationSyntax>() is not null;

    private static bool HasNamespaceAncestor(MethodDeclarationSyntax methodDeclaration)
        => methodDeclaration.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>() is not null;

    // No diagnostic if the signature is valid.
    private static bool IsValidSignature(MethodDeclarationSyntax methodDeclaration)
        => methodDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.AsyncKeyword)) &&
           methodDeclaration.ReturnType is IdentifierNameSyntax ReturnTypeName &&
           ReturnTypeName.Identifier.Text == "Task" &&
           methodDeclaration.Identifier.Text.EndsWith("Async", StringComparison.Ordinal);

    // No diagnostic if there is no or many argument.
    private static bool IsValidArguments(AttributeSyntax attribute)
        => attribute.ArgumentList is not AttributeArgumentListSyntax AttributeArgumentList || AttributeArgumentList.Arguments.Count > 0;
}
