namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<CSL1005SimplifyOneLineGetter, CSL1005CodeFixProvider>;

public partial class CSL1005UnitTests
{
    [TestMethod]
    public async Task CoverageDirective_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952
" + Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get { return ""Test""; } }
    }
");
    }

    [TestMethod]
    public async Task OldLanguageVersion_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Default, @"
    class Program
    {
        public string Prop { get { return ""Test""; } }
    }
", Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp5);
    }

    [TestMethod]
    public async Task OldLanguageVersionWithSetter_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Default, @"
    class Program
    {
        public string Prop { get { return ""Test""; } set { } }
    }
", Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp6);
    }
}
