#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<Analyzers::CSharpLatest.CSL1010InitAccessorNotSupportedInFieldBackedPropertyAttribute>;

internal partial class CSL1010UnitTests
{
    [TestMethod]
    public async Task CoverageDirective_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952
" + Prologs.IsExternalInit, @"

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"")]
    public int Test { get; init; }
}
").ConfigureAwait(false);
    }
}
