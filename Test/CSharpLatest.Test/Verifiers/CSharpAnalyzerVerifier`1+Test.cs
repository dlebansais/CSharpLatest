#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using NuGet.Configuration;

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
                string ContractAssemblyPath = GetContractAssemblyPath();
                PortableExecutableReference ReferenceContracts = MetadataReference.CreateFromFile(@"C:\Users\DLB\.nuget\packages\dlebansais.csharplatest\2.0.0\analyzers\dotnet\cs\CSharpLatest.Analyzers.dll");

                List<MetadataReference> DefaultReferences =
                [
                    //MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                    ReferenceContracts,
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "mscorlib")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "System")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "System.Core")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "System.Xaml")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "PresentationCore")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, "PresentationFramework")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, @"Facades\System.Runtime")),
                    MetadataReference.CreateFromFile(string.Format(CultureInfo.InvariantCulture, RuntimePath, @"Facades\System.Collections")),
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

        private static string GetContractAssemblyPath()
        {
#if DEBUG
            string AssemblyPath = GetContractAssemblyPath("dlebansais.CSharpLatest-Debug");
            if (File.Exists(AssemblyPath))
                return AssemblyPath;
#endif

            return GetContractAssemblyPath("dlebansais.CSharpLatest");
        }

        private static string GetContractAssemblyPath(string assemblyName)
        {
            ISettings settings = Settings.LoadDefaultSettings(null);
            string nugetPath = SettingsUtility.GetGlobalPackagesFolder(settings);
            Version AssemblyVersion = typeof(PropertyAttribute).Assembly.GetName().Version!;
            string AssemblyVersionString = $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}.{AssemblyVersion.Build}";
            string AssemblyPath = Path.Combine(nugetPath, assemblyName, AssemblyVersionString, "lib", "net481", "CSharpLatest.CSharpAnalyzers.dll");

            return AssemblyPath;
        }
    }
}
