namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<CSL1006SimplifyOneLineSetter, CSL1006CodeFixProvider>;

public partial class CSL1006UnitTests
{
    [TestMethod]
    public async Task CoverageDirective_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952
" + Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { set { _prop = value; } }
        private string _prop = string.Empty;
    }
");
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
", Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp6);
    }
}
