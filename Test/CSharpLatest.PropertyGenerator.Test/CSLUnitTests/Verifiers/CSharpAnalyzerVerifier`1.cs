namespace CSharpLatest.Test;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

internal static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic()"/>
    public static DiagnosticResult Diagnostic()
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic();

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(string)"/>
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(diagnosticId);

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(descriptor);

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
    public static async Task VerifyAnalyzerAsync(string source, LanguageVersion languageVersion = LanguageVersion.Default, params IEnumerable<DiagnosticResult> expected)
        => await VerifyAnalyzerAsync([], Prologs.Default, source, languageVersion, expected).ConfigureAwait(false);

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
    public static async Task VerifyAnalyzerAsync(string prolog, string source, LanguageVersion languageVersion = LanguageVersion.Default, params IEnumerable<DiagnosticResult> expected)
        => await VerifyAnalyzerAsync([], prolog, source, languageVersion, expected).ConfigureAwait(false);

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
    public static async Task VerifyAnalyzerAsync(Dictionary<string, string> options, string prolog, string source, LanguageVersion languageVersion = LanguageVersion.Default, params IEnumerable<DiagnosticResult> expected)
    {
        Test test = new();

        if (test.IsDiagnosticEnabledd && test.HasHelpLink)
        {
            test = new()
            {
                TestCode = prolog + source,
                Version = languageVersion,
                Options = options,
            };
        }

        test.ExpectedDiagnostics.AddRange(expected);

        await test.RunAsync(CancellationToken.None).ConfigureAwait(false);
    }
}
