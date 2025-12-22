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
using Microsoft.CodeAnalysis.Operations;

/// <summary>
/// Analyzer for rule CSL1011: Implement params collection.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1011ImplementParamsCollection : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1011";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1011AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1011AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1011AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
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

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Parameter);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context) => AnalyzerTools.AssertSyntaxRequirements<ParameterSyntax>(context, LanguageVersion.CSharp13, AnalyzeVerifiedNode);

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, ParameterSyntax parameter, IEnumerable<IAnalysisAssertion> analysisAssertions)
    {
        // We handle only methods, not indexers or constructors.
        if (parameter.FirstAncestorOrSelf<MemberDeclarationSyntax>() is not MethodDeclarationSyntax MethodDeclaration)
            return;

        // We only handle 'params'.
        if (!IsParams(parameter))
            return;

        // We only handle T[]
        if (parameter.Type is not ArrayTypeSyntax)
            return;

        // .NET Framework and .NET Standard older than 2.1 don't have ReadOnlySpan.
        if (!IsValidFramework(context.Compilation))
            return;

        // Get the method symbol (I could not figure out a way to get null here).
        IMethodSymbol methodSymbol = Contract.AssertNotNull(context.SemanticModel.GetDeclaredSymbol(MethodDeclaration, context.CancellationToken));

        // No diagnostic if the ReadOnlySpan<T> override already exists.
        int ParameterIndex = MethodDeclaration.ParameterList.Parameters.IndexOf(parameter);
        IEnumerable<ISymbol> Overrides = methodSymbol.ContainingType.GetMembers().Where(member => IsReadOnlySpanOverride(methodSymbol, ParameterIndex, member));
        if (Overrides.Any())
            return;

        if (!IsUsed(context, MethodDeclaration, parameter))
            return;

        Location Location = parameter.GetLocation();

        context.ReportDiagnostic(Diagnostic.Create(Rule, Location, methodSymbol.Name));
    }

    private static bool IsUsed(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodDeclaration, ParameterSyntax parameter)
    {
        SyntaxNode Body;
        if (methodDeclaration.Body is BlockSyntax MethodBlock)
            Body = MethodBlock;
        else if (methodDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ExpressionBody)
            Body = ExpressionBody;
        else
            return false;

        // Get the method operation (I could not figure out a way to get null here).
        IOperation Operation = Contract.AssertNotNull(context.SemanticModel.GetOperation(Body, context.CancellationToken));

        // Get the parameter symbol (I could not figure out a way to get null here).
        IParameterSymbol ParameterSymbol = Contract.AssertNotNull(context.SemanticModel.GetDeclaredSymbol(parameter, context.CancellationToken));

#pragma warning disable IDE0046 // Convert to conditional expression
        if (!IsSymbolUsed(Operation, ParameterSymbol))
            return false;
#pragma warning restore IDE0046 // Convert to conditional expression

        return true;
    }

    private static bool IsValidFramework(Compilation compilation)
    {
        bool IsDotNetFramework = AnalyzerTools.IsDotNetFramework(compilation);
        bool IsDotNet = AnalyzerTools.IsDotNet(compilation);
        bool IsLastDotNetStandard = AnalyzerTools.IsDotNetStandard(compilation, minimumVersion: new Version(2, 1));

        return !IsDotNetFramework && (IsDotNet || IsLastDotNetStandard);
    }

    private static bool IsParams(ParameterSyntax parameter)
    {
        if (parameter.Modifiers.Count != 1)
            return false;

        SyntaxToken Modifier = parameter.Modifiers[0];
        return Modifier.IsKind(SyntaxKind.ParamsKeyword);
    }

    private static bool IsReadOnlySpanOverride(IMethodSymbol methodSymbol, int parameterIndex, ISymbol symbol)
    {
        Contract.Assert(parameterIndex < methodSymbol.Parameters.Length);

        if (symbol is not IMethodSymbol MethodMember)
            return false;

        if (MethodMember.Parameters.Length != methodSymbol.Parameters.Length)
            return false;

        IParameterSymbol ParameterSymbol = MethodMember.Parameters[parameterIndex];

        if (!ParameterSymbol.IsParams)
            return false;

        ITypeSymbol ParameterTypeSymbol = ParameterSymbol.Type;
        return ParameterTypeSymbol is INamedTypeSymbol NamedTypeSymbol &&
               NamedTypeSymbol.Name == "ReadOnlySpan" &&
               MethodMember.Name == methodSymbol.Name;
    }

    private static bool IsSymbolUsed(IOperation operation, IParameterSymbol parameterSymbol)
    {
        foreach (IOperation op in operation.DescendantsAndSelf())
        {
            if (op is IParameterReferenceOperation paramRef && SymbolEqualityComparer.Default.Equals(paramRef.Parameter, parameterSymbol))
            {
                return true;
            }
        }

        return false;
    }
}
