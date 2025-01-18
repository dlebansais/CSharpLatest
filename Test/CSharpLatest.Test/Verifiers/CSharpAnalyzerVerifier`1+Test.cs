#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

internal static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public class Test : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
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
                solution = solution.WithProjectCompilationOptions(projectId, CompilationOptions ?? throw new NullReferenceException());

                string RuntimePath = GetRuntimePath();

                List<MetadataReference> DefaultReferences =
                [
                    MetadataReference.CreateFromFile("CSharpLatest.Attributes.dll"),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "mscorlib")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "System")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "System.Core")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "System.Xaml")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "PresentationCore")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "PresentationFramework")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, @"Facades\System.Runtime")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, @"Facades\System.Collections")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, @"Facades\netstandard")),
                ];

                solution = solution.WithProjectMetadataReferences(projectId, DefaultReferences);

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

        public bool IsDiagnosticEnabledd
        {
            get
            {
                DiagnosticAnalyzer[] Analyzers = GetDiagnosticAnalyzers().ToArray();
                DiagnosticDescriptor? Diagnostics = GetDefaultDiagnostic(Analyzers);
                return Diagnostics is not null && Diagnostics.IsEnabledByDefault;
            }
        }

        public bool HasHelpLink
        {
            get
            {
                DiagnosticAnalyzer[] Analyzers = GetDiagnosticAnalyzers().ToArray();
                DiagnosticDescriptor? Diagnostics = GetDefaultDiagnostic(Analyzers);
                return Diagnostics is not null && Diagnostics.HelpLinkUri is string Uri && Uri.StartsWith("https://github.com/dlebansais/CSharpLatest", StringComparison.Ordinal);
            }
        }

        public Dictionary<string, string> Options { get; set; } = [];

        private static string GetRuntimePath()
        {
            const string RuntimeDirectoryBase = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework";
            string RuntimeDirectory = string.Empty;

            foreach (string FolderPath in GetRuntimeDirectories(RuntimeDirectoryBase))
                if (IsValidRuntimeDirectory(FolderPath))
                    RuntimeDirectory = FolderPath;

            string RuntimePath = RuntimeDirectory + @"\{0}.dll";

            return RuntimePath;
        }

        private static List<string> GetRuntimeDirectories(string runtimeDirectoryBase)
        {
            string[] Directories = Directory.GetDirectories(runtimeDirectoryBase);
            List<string> DirectoryList = [.. Directories];
            DirectoryList.Sort(CompareIgnoreCase);

            return DirectoryList;
        }

        private static int CompareIgnoreCase(string s1, string s2) => string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase);

        private static bool IsValidRuntimeDirectory(string folderPath)
        {
            string FolderName = Path.GetFileName(folderPath);
            const string Prefix = "v";

            Contract.Assert(FolderName.StartsWith(Prefix, StringComparison.Ordinal));

            string[] Parts = FolderName[Prefix.Length..].Split('.');
            foreach (string Part in Parts)
                if (!int.TryParse(Part, out _))
                    return false;

            return true;
        }
    }
}
