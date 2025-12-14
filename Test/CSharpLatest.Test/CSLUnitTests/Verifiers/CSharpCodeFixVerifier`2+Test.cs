#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

internal static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    internal class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    {
        public Test()
        {
            SolutionTransforms.Add((solution, projectId) =>
            {
                List<KeyValuePair<string, ReportDiagnostic>> CustomOptions = [];
                foreach (KeyValuePair<string, string> Entry in Options)
                    CustomOptions.Add(new KeyValuePair<string, ReportDiagnostic>($"{Entry.Key}={Entry.Value}", ReportDiagnostic.Default));

                CompilationOptions? CompilationOptions = solution.GetProject(projectId)?.CompilationOptions;
                CompilationOptions = CompilationOptions?.WithSpecificDiagnosticOptions(CompilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
                CompilationOptions = CompilationOptions?.WithSpecificDiagnosticOptions(CustomOptions);
                solution = solution.WithProjectCompilationOptions(projectId, CompilationOptions ?? throw new InvalidOperationException());

                if (Version != LanguageVersion.Default)
                {
                    CSharpParseOptions? ParseOptions = (CSharpParseOptions?)solution.GetProject(projectId)?.ParseOptions;
                    ParseOptions = ParseOptions?.WithLanguageVersion(Version);
                    solution = solution.WithProjectParseOptions(projectId, ParseOptions ?? throw new InvalidOperationException());
                }

                if (FrameworkChoiceInternal == FrameworkChoice.None)
                {
                    List<MetadataReference> DefaultReferences =
                    [
                        MetadataReference.CreateFromFile("CSharpLatest.Attributes.dll"),
                    ];

                    solution = solution.WithProjectMetadataReferences(projectId, DefaultReferences);
                }

                return solution;
            });

            TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile("CSharpLatest.Attributes.dll"));
        }

        public LanguageVersion Version { get; set; } = LanguageVersion.Default;

        private FrameworkChoice FrameworkChoiceInternal = FrameworkChoice.Default;

        public FrameworkChoice FrameworkChoice
        {
            get => FrameworkChoiceInternal;
            set
            {
                FrameworkChoiceInternal = value;

                switch (value)
                {
                    case FrameworkChoice.DotNetStandard:
                        ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard21;
                        break;
                    case FrameworkChoice.DotNetFramework:
                        ReferenceAssemblies = ReferenceAssemblies.NetFramework.Net472.Wpf;
                        break;
                    case FrameworkChoice.OldDotNetStandard:
                        ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard20;
                        break;
                    case FrameworkChoice.DotNet8:
                        ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
                        break;
                    case FrameworkChoice.DotNet9:
                        ReferenceAssemblies = ReferenceAssemblies.Net.Net90;
                        break;

                    case FrameworkChoice.Default:
                    case FrameworkChoice.None:
                    default:
                        break;
                }
            }
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

        public Dictionary<string, string> Options { get; set; } = [];
    }
}
