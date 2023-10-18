﻿namespace CSharpLatest;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynHelpers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CSL1001Analyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CSL1001";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.CSL1001AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.CSL1001AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.CSL1001AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.EqualsExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var BinaryExpression = (BinaryExpressionSyntax)context.Node;

        // Check whether the comparison is '== null'.
        if (BinaryExpression.Right is not LiteralExpressionSyntax literalExpressionRight)
            return;
        if (!BinaryExpression.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken))
            return;
        if (!literalExpressionRight.IsKind(SyntaxKind.NullLiteralExpression))
            return;

        // Get the expression type.
        ITypeSymbol? ExpressionType = BinaryExpression.Left.GetExpressionValidType(context);

        // If there is an error, stop analyzing.
        if (ExpressionType is null)
            return;

        // If the == operator is overloaded, 'is null' behaves differently. Do not replace.
        if (ExpressionType.IsOverloadingEqualsOperator(context))
            return;

        // If a value type, and not nullable, the comparison with null is going to generate an error. Do not emit a diagnostic.
        if (!ExpressionType.IsReferenceType && ExpressionType.NullableAnnotation != NullableAnnotation.Annotated)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{BinaryExpression.OperatorToken} {literalExpressionRight}"));
    }
}
