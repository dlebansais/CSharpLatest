namespace CSharpLatest;

using System.Collections.Immutable;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynHelpers;

/// <summary>
/// Analyzer for rule CLS1002: Use 'is not null' syntax instead of '!= null'.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class CSL1002UseIsNotNull : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1002";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1002AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1002AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1002AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
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
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return [Rule]; } }

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

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.NotEqualsExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        AnalyzerTools.AssertSyntaxRequirements<BinaryExpressionSyntax>(context,
            LanguageVersion.CSharp9,
            AnalyzeVerifiedNode,
            new SimpleAnalysisAssertion(context => ((BinaryExpressionSyntax)context.Node).OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken)));
    }

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, BinaryExpressionSyntax binaryExpression, IAnalysisAssertion[] analysisAssertions)
    {
        var RightExpression = binaryExpression.Right;
        var OperatorToken = binaryExpression.OperatorToken;
        var LeftExpression = binaryExpression.Left;

        // Check whether the comparison is '!= null'.
        if (RightExpression is not LiteralExpressionSyntax literalExpressionRight)
            return;
        if (!literalExpressionRight.IsKind(SyntaxKind.NullLiteralExpression))
            return;

        // Get the expression type.
        ITypeSymbol? ExpressionType = LeftExpression.GetExpressionValidType(context);

        // If there is an error, stop analyzing.
        if (ExpressionType is null)
            return;

        // If the != operator is overloaded, 'is not null' behaves differently. Do not replace.
        if (ExpressionType.IsOverloadingExclamationEqualsOperator(context))
            return;

        // If a value type, and not nullable, the comparison with null is going to generate an error. Do not emit a diagnostic.
        if (!ExpressionType.IsReferenceType && ExpressionType.NullableAnnotation != NullableAnnotation.Annotated)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{OperatorToken} {literalExpressionRight}"));
    }
}
