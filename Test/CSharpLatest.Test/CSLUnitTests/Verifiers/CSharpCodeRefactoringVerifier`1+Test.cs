#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

internal static partial class CSharpCodeRefactoringVerifier<TCodeRefactoring>
    where TCodeRefactoring : CodeRefactoringProvider, new()
{
    internal class Test : CSharpCodeRefactoringTest<TCodeRefactoring, DefaultVerifier>
    {
        public Test()
        {
            SolutionTransforms.Add((solution, projectId) =>
            {
                CompilationOptions? compilationOptions = solution.GetProject(projectId)?.CompilationOptions;
                compilationOptions = compilationOptions?.WithSpecificDiagnosticOptions(compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
                solution = solution.WithProjectCompilationOptions(projectId, compilationOptions ?? throw new InvalidOperationException());

                return solution;
            });
        }

        public bool IsDiagnosticEnabledd
        {
            get
            {
                DiagnosticAnalyzer[] Analyzers = [.. GetDiagnosticAnalyzers()];
                DiagnosticDescriptor? Diagnostics = GetDefaultDiagnostic(Analyzers);
                return Diagnostics is not null && Diagnostics.IsEnabledByDefault;
            }
        }

        public bool HasHelpLink
        {
            get
            {
                DiagnosticAnalyzer[] Analyzers = [.. GetDiagnosticAnalyzers()];
                DiagnosticDescriptor? Diagnostics = GetDefaultDiagnostic(Analyzers);
                return Diagnostics is not null && Diagnostics.HelpLinkUri is string Uri && Uri.StartsWith("https://github.com/dlebansais/CSharpLatest", StringComparison.Ordinal);
            }
        }
    }
}
