#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1007AddMissingBraces, CSL1007CodeFixProvider>;

internal partial class CSL1007UnitTests
{
    [TestMethod]
    public async Task CoverageDirective_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952
" + Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
            return 1;

        return 0;
    }
}
").ConfigureAwait(false);
    }
}
