namespace CSharpLatest;

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynHelpers;

/// <summary>
/// Analyzer for rule CSL1003: Consider using primary constructor.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CSL1003ConsiderUsingPrimaryConstructor : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic ID for this rule.
    /// </summary>
    public const string DiagnosticId = "CSL1003";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.CSL1003AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.CSL1003AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.CSL1003AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,
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
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        AnalyzerTools.AssertSyntaxRequirements<ClassDeclarationSyntax>(context, LanguageVersion.CSharp12, AnalyzeVerifiedNode);
    }

    private void AnalyzeVerifiedNode(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration, IAnalysisAssertion[] analysisAssertions)
    {
        // Make sure the class has a name to display.
        if (classDeclaration.Identifier.Text == string.Empty)
            return;

        // Make sure the declaration doesn't already use a primary contructor.
        if (classDeclaration.ParameterList is not null)
            return;

        // If the list of candidates is empty, no primary constructor can be used.
        List<ParameterSyntax> ParameterCandidates = GetParameterCandidates(classDeclaration);
        if (ParameterCandidates.Count == 0)
            return;

        // If there isn't a constructor that handles these parameters only, let's not try to second-guess the code.
        if (GetConstructorCandidate(classDeclaration, ParameterCandidates) is not ConstructorDeclarationSyntax ConstructorCandidate)
            return;

        // If the constructor is doing anything else than assigning properties, let's not try to second-guess the code.
        if (!IsConstructorOnlyAssigningProperties(ConstructorCandidate))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), classDeclaration.Identifier.Text));
    }

    private static List<ParameterSyntax> GetParameterCandidates(ClassDeclarationSyntax classDeclaration)
    {
        List<ParameterSyntax> Result = new();
        bool IsFirstConstructor = true;

        foreach (var Member in classDeclaration.Members)
            if (Member is ConstructorDeclarationSyntax ConstructorDeclaration)
            {
                List<ParameterSyntax> Parameters = [.. ConstructorDeclaration.ParameterList.Parameters];

                if (IsFirstConstructor)
                {
                    IsFirstConstructor = false;
                    Result = [.. Parameters];
                }
                else
                {
                    int LastCandidateIndex;

                    for (LastCandidateIndex = 0; LastCandidateIndex < Result.Count && LastCandidateIndex < Parameters.Count; LastCandidateIndex++)
                        if (!IsSameParameter(Result[LastCandidateIndex], Parameters[LastCandidateIndex]))
                            break;

                    Result = Result.GetRange(0, LastCandidateIndex);
                }
            }

        return Result;
    }

    private static bool IsSameParameter(ParameterSyntax p1, ParameterSyntax p2)
    {
        bool IsEqual = p1.IsEquivalentTo(p2);

        return IsEqual;
    }

    private static ConstructorDeclarationSyntax? GetConstructorCandidate(ClassDeclarationSyntax classDeclaration, List<ParameterSyntax> parameterCandidates)
    {
        foreach (var Member in classDeclaration.Members)
            if (Member is ConstructorDeclarationSyntax ConstructorDeclaration)
                if (ConstructorDeclaration.ParameterList.Parameters.Count == parameterCandidates.Count)
                    return ConstructorDeclaration;

        return null;
    }

    private static bool IsConstructorOnlyAssigningProperties(ConstructorDeclarationSyntax constructorDeclaration)
    {
        List<AssignmentExpressionSyntax> Assignments = new();

        if (constructorDeclaration.Body is BlockSyntax Body)
        {
            foreach (var Statement in Body.Statements)
            {
                if (Statement is not ExpressionStatementSyntax ExpressionStatement || ExpressionStatement.Expression is not AssignmentExpressionSyntax Assignment)
                    return false;

                Assignments.Add(Assignment);
            }
        }

        if (constructorDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ExpressionBody)
        {
            if (ExpressionBody.Expression is AssignmentExpressionSyntax Assignment)
                Assignments = new() { Assignment };
        }

        return true;
    }
}
