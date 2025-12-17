#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

#if ENABLE_CSL1013

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1013ChangeExtensionFunctionToExtensionMember, CSL1013CodeFixProvider>;

internal partial class CSL1013UnitTests
{
    [TestMethod]
    public async Task CoverageDirective_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952
" + Prologs.Default, @"
static class Program
{
    public static int GetLength(this string s)
    {
        ArgumentNullException.ThrowIfNull(s);

        return s.Length;
    }
}
", LanguageVersion.CSharp14, FrameworkChoice.DotNet10).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OldLanguageVersion_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
static class Program
{
    public static int GetLength(this string s)
    {
        ArgumentNullException.ThrowIfNull(s);

        return s.Length;
    }
}
", LanguageVersion.CSharp13, FrameworkChoice.DotNet10).ConfigureAwait(false);
    }
}

#endif
