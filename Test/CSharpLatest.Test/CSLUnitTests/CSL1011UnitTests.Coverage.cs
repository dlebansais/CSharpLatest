#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1011ImplementParamsCollection, CSL1011CodeFixProvider>;

internal partial class CSL1011UnitTests
{
    [TestMethod]
    public async Task CoverageDirective_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952
" + Prologs.IsExternalInitNoNullable, @"
    class Program
    {
        public static int Foo<T>(params T[] items)
        {
            int result = items.Length;
            return result;
        }
    }
", Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp13).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OldLanguageVersion_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Default, @"
    class Program
    {
        public static int Foo<T>(params T[] items)
        {
            int result = items.Length;
            return result;
        }
    }
", Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp12).ConfigureAwait(false);
    }
}
