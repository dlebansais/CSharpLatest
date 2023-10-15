namespace CSharpLatest;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

        // If the == oprator is overloaded, 'is null' behaves differently. Do not replace.
        if (IsEqualsOverloaded(context, BinaryExpression.Left, referenceTypeOnly: true))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{BinaryExpression.OperatorToken} {literalExpressionRight}"));
    }

    private static bool IsEqualsOverloaded(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, bool referenceTypeOnly)
    {
        ITypeSymbol? ExpressionType = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken).Type;
        if (ExpressionType is not null && (!referenceTypeOnly || ExpressionType.IsReferenceType))
            return IsEqualsOverloaded(context, ExpressionType);

        return false;
    }

    private static bool IsEqualsOverloaded(SyntaxNodeAnalysisContext context, ITypeSymbol expressionType)
    {
        foreach (var Symbol in expressionType.GetMembers())
            if (!Symbol.IsImplicitlyDeclared && IsOverloadOfEquals(context, Symbol))
                return true;

        return false;
    }

    private static bool IsOverloadOfEquals(SyntaxNodeAnalysisContext context, ISymbol symbol)
    {
        foreach (var SyntaxReference in symbol.DeclaringSyntaxReferences)
        {
            SyntaxNode Declaration = SyntaxReference.GetSyntax(context.CancellationToken);

            if (Declaration is OperatorDeclarationSyntax OperatorDeclaration && OperatorDeclaration.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken))
                return true;
        }

        return false;
    }
}
