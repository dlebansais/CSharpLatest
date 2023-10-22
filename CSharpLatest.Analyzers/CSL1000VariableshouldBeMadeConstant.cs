namespace CSharpLatest;

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynHelpers;

/// <summary>
/// Analyzer for rule CLS1000: Variables that are not modified should be made constants.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CSL1000VariableshouldBeMadeConstant : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1000";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1000AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1000AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1000AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    /// <summary>
    /// Gets the list of supported diagnostic.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    /// <summary>
    /// Initializes the rule analyzer.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.LocalDeclarationStatement);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        AnalyzerTools.AssertSyntaxRequirements<LocalDeclarationStatementSyntax>(context, AnalyzeVerifiedNode, new DataFlowAnalysisAssertion<LocalDeclarationStatementSyntax>());
    }

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, LocalDeclarationStatementSyntax localDeclaration, IAnalysisAssertion[] analysisAssertions)
    {
        // Make sure the declaration isn't already const.
        if (localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
            return;

        TypeSyntax variableTypeName = localDeclaration.Declaration.Type;
        ITypeSymbol? VariableType = variableTypeName.GetTypeValidType(context);

        // Ensure that all variables in the local declaration have initializers that are assigned with constant values.
        foreach (VariableDeclaratorSyntax variable in localDeclaration.Declaration.Variables)
            if (!IsVariableAssignedToConstantValue(context, VariableType, variable))
                return;

        // Gets the data flow analysis performed on the local declaration during the analysis assertion phase.
        DataFlowAnalysis DataFlowAnalysis = ((DataFlowAnalysisAssertion<LocalDeclarationStatementSyntax>)analysisAssertions.First()).DataFlowAnalysis;

        foreach (VariableDeclaratorSyntax variable in localDeclaration.Declaration.Variables)
        {
            // Retrieve the local symbol for each variable in the local declaration and ensure that it is not written outside of the data flow analysis region.
            ISymbol? variableSymbol = context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken);
            if (variableSymbol is not null && DataFlowAnalysis.WrittenOutside.Contains(variableSymbol))
                return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), localDeclaration.Declaration.Variables.First().Identifier.ValueText));
    }

    private bool IsVariableAssignedToConstantValue(SyntaxNodeAnalysisContext context, ITypeSymbol? variableType, VariableDeclaratorSyntax variable)
    {
        EqualsValueClauseSyntax? initializer = variable.Initializer;
        if (initializer is null)
            return false;

        Optional<object?> constantValue = context.SemanticModel.GetConstantValue(initializer.Value, context.CancellationToken);
        if (!constantValue.HasValue)
            return false;

        if (variableType is not null)
        {
            // Ensure that the initializer value can be converted to the type of the local declaration without a user-defined conversion.
            Conversion conversion = context.SemanticModel.ClassifyConversion(initializer.Value, variableType);
            if (!conversion.Exists || conversion.IsUserDefined)
                return false;

            // Special cases:
            //  * If the constant value is a string, the type of the local declaration must be System.String.
            //  * If the constant value is null, the type of the local declaration must be a reference type.
            if (constantValue.Value is string)
            {
                if (variableType.SpecialType != SpecialType.System_String)
                    return false;
            }
            else if (variableType.IsReferenceType && constantValue.Value is not null)
                return false;
        }

        return true;
    }
}
