#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1015DoNotDeclareAsyncVoidMethods, CSL1015CodeFixProvider>;

internal partial class CSL1015UnitTests
{
    [TestMethod]
    public async Task CoverageDirective_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952
" + Prologs.Default, @"
class Program
{
    async void Foo()
    {
    }
}
", LanguageVersion.CSharp5).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OldLanguageVersion_NoDiagnostic()
    {
        DiagnosticDescriptor DescriptorCS8025 = new(
            "CS8025",
            "title",
            "Feature 'async function' is not available in C# 4. Please use language version 5 or greater.",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected = new(DescriptorCS8025);
        Expected = Expected.WithLocation("/0/Test0.cs", 9, 16);

        await VerifyCS.VerifyAnalyzerAsync(@"
class Program
{
    async void Foo()
    {
    }
}
", LanguageVersion.CSharp4, FrameworkChoice.Default, Expected).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoNameUsing_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync("", @"
using TaskTuple = (System.Threading.Tasks.Task, System.Threading.Tasks.Task);

class Program
{
    [|async void Foo()
    {
    }|]
}
", @"
using TaskTuple = (System.Threading.Tasks.Task, System.Threading.Tasks.Task);
using System.Threading.Tasks;

class Program
{
    async Task Foo()
    {
    }
}
", LanguageVersion.CSharp12).ConfigureAwait(false);
    }
}
