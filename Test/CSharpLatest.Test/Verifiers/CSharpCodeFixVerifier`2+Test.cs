using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace CSharpLatest.Test;

public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    {
        public Test()
        {
            SolutionTransforms.Add((solution, projectId) =>
            {
                List<KeyValuePair<string, ReportDiagnostic>> CustomOptions = [];
                foreach (KeyValuePair<string, string> Entry in Options)
                    CustomOptions.Add(new KeyValuePair<string, ReportDiagnostic>($"{Entry.Key}={Entry.Value}", ReportDiagnostic.Default));

                var CompilationOptions = solution.GetProject(projectId)?.CompilationOptions;
                CompilationOptions = CompilationOptions?.WithSpecificDiagnosticOptions(CompilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
                CompilationOptions = CompilationOptions?.WithSpecificDiagnosticOptions(CustomOptions);

                solution = solution.WithProjectCompilationOptions(projectId, CompilationOptions ?? throw new NullReferenceException());

                if (Version != LanguageVersion.Default)
                {
                    CSharpParseOptions? ParseOptions = (CSharpParseOptions?)solution.GetProject(projectId)?.ParseOptions;
                    ParseOptions = ParseOptions?.WithLanguageVersion(Version);
                    solution = solution.WithProjectParseOptions(projectId, ParseOptions ?? throw new NullReferenceException());
                }

                return solution;
            });
        }

        public LanguageVersion Version { get; set; } = LanguageVersion.Default;
        public Dictionary<string, string> Options { get; set; } = [];
    }
}
