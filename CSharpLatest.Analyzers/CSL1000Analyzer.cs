﻿namespace CSharpLatest;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer for rule CLS1000: Variables that are not modified should be made constants.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CSL1000Analyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1000";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.CSL1000AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.CSL1000AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.CSL1000AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
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
        var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;

        // make sure the declaration isn't already const:
        if (localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
            return;

        TypeSyntax variableTypeName = localDeclaration.Declaration.Type;
        ITypeSymbol? variableType = context.SemanticModel.GetTypeInfo(variableTypeName, context.CancellationToken).ConvertedType;

        // Ensure that all variables in the local declaration have initializers that
        // are assigned with constant values.
        foreach (VariableDeclaratorSyntax variable in localDeclaration.Declaration.Variables)
        {
            EqualsValueClauseSyntax? initializer = variable.Initializer;
            if (initializer is null)
                return;

            Optional<object?> constantValue = context.SemanticModel.GetConstantValue(initializer.Value, context.CancellationToken);
            if (!constantValue.HasValue)
                return;

            if (variableType is not null)
            {
                // Ensure that the initializer value can be converted to the type of the local declaration without a user-defined conversion.
                Conversion conversion = context.SemanticModel.ClassifyConversion(initializer.Value, variableType);
                if (!conversion.Exists || conversion.IsUserDefined)
                    return;

                // Special cases:
                //  * If the constant value is a string, the type of the local declaration must be System.String.
                //  * If the constant value is null, the type of the local declaration must be a reference type.
                if (constantValue.Value is string)
                {
                    if (variableType.SpecialType != SpecialType.System_String)
                        return;
                }
                else if (variableType.IsReferenceType && constantValue.Value is not null)
                    return;
            }
        }

        // Perform data flow analysis on the local declaration.
        DataFlowAnalysis? dataFlowAnalysis = context.SemanticModel.AnalyzeDataFlow(localDeclaration);

        foreach (VariableDeclaratorSyntax variable in localDeclaration.Declaration.Variables)
        {
            // Retrieve the local symbol for each variable in the local declaration
            // and ensure that it is not written outside of the data flow analysis region.
            ISymbol? variableSymbol = context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken);
            if (dataFlowAnalysis is not null && variableSymbol is not null && dataFlowAnalysis.WrittenOutside.Contains(variableSymbol))
                return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), localDeclaration.Declaration.Variables.First().Identifier.ValueText));
    }
}