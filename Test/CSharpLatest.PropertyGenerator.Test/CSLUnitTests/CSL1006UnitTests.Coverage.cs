#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1006SimplifyOneLineSetter, CSL1006CodeFixProvider>;

internal partial class CSL1006UnitTests
{
    [TestMethod]
    public async Task CoverageDirective_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952
" + Prologs.IsExternalInitNoNullable, @"
    class Program
    {
        public string Prop { set { _prop = value; } }
        private string _prop = string.Empty;
    }
", Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp7).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OldLanguageVersion_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Default, @"
    class Program
    {
        public string Prop { set { _prop = value; } }
        private string _prop = string.Empty;
    }
", Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp6).ConfigureAwait(false);
    }
}
