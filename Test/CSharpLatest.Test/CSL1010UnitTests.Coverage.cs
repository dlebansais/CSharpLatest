#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<CSL1010InitAccessorNotSupportedInFieldBackedPropertyAttribute>;

public partial class CSL1010UnitTests
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
