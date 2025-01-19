namespace CSharpLatest.Test;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic()"/>
    public static DiagnosticResult Diagnostic()
        => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic();

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(string)"/>
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(diagnosticId);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(descriptor);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
    public static async Task VerifyAnalyzerAsync(string source, LanguageVersion languageVersion = LanguageVersion.Default, params IEnumerable<DiagnosticResult> expected)
        => await VerifyAnalyzerAsync([], Prologs.Default, source, languageVersion, expected).ConfigureAwait(false);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
    public static async Task VerifyAnalyzerAsync(string prolog, string source, LanguageVersion languageVersion = LanguageVersion.Default, params IEnumerable<DiagnosticResult> expected)
        => await VerifyAnalyzerAsync([], prolog, source, languageVersion, expected).ConfigureAwait(false);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
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

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, string)"/>
    public static async Task VerifyCodeFixAsync(string source, string fixedSource, LanguageVersion languageVersion = LanguageVersion.Default)
        => await VerifyCodeFixAsync([], Prologs.Default, source, DiagnosticResult.EmptyDiagnosticResults, fixedSource, languageVersion).ConfigureAwait(false);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, string)"/>
    public static async Task VerifyCodeFixAsync(string prolog, string source, string fixedSource, LanguageVersion languageVersion = LanguageVersion.Default)
        => await VerifyCodeFixAsync([], prolog, source, DiagnosticResult.EmptyDiagnosticResults, fixedSource, languageVersion).ConfigureAwait(false);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, string)"/>
    public static async Task VerifyCodeFixAsync(Dictionary<string, string> options, string source, string fixedSource, LanguageVersion languageVersion = LanguageVersion.Default)
        => await VerifyCodeFixAsync(options, Prologs.Default, source, DiagnosticResult.EmptyDiagnosticResults, fixedSource, languageVersion).ConfigureAwait(false);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, string)"/>
    public static async Task VerifyCodeFixAsync(Dictionary<string, string> options, string prolog, string source, string fixedSource, LanguageVersion languageVersion = LanguageVersion.Default)
        => await VerifyCodeFixAsync(options, prolog, source, DiagnosticResult.EmptyDiagnosticResults, fixedSource, languageVersion).ConfigureAwait(false);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult, string)"/>
    public static async Task VerifyCodeFixAsync(Dictionary<string, string> options, string prolog, string source, DiagnosticResult expected, string fixedSource, LanguageVersion languageVersion = LanguageVersion.Default)
        => await VerifyCodeFixAsync(options, prolog, source, [expected], fixedSource, languageVersion).ConfigureAwait(false);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult[], string)"/>
    public static async Task VerifyCodeFixAsync(Dictionary<string, string> options, string prolog, string source, DiagnosticResult[] expected, string fixedSource, LanguageVersion languageVersion = LanguageVersion.Default)
    {
        Test test = new();

        if (test.IsDiagnosticEnabledd && test.HasHelpLink)
        {
            test = new()
            {
                TestCode = (prolog + source).Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", "\r\n", StringComparison.Ordinal),
                FixedCode = (prolog + fixedSource).Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", "\r\n", StringComparison.Ordinal),
                Version = languageVersion,
                Options = options,
            };
        }

        test.ExpectedDiagnostics.AddRange(expected);

        await test.RunAsync(CancellationToken.None).ConfigureAwait(false);
    }
}
