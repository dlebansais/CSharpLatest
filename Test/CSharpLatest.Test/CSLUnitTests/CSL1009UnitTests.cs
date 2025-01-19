#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<Analyzers::CSharpLatest.CSL1009FieldBackedPropertyAttributeIsMissingArgument>;

[TestClass]
internal partial class CSL1009UnitTests
{
    [TestMethod]
    public async Task NoArgumentProperty_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal partial class Program
{
    [[|FieldBackedProperty|]]
    public int Test { get; }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OneArgument_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
namespace TestSuite;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"")]
    public int Test { get; }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task EmptyArgument_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
internal partial class Program
{
    [[|FieldBackedProperty()|]]
    public int Test { get; }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task UnsupportedAttribute_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
internal partial class Program
{
    [Obsolete]
    public int Test { get; }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoArgumentOtherProperty_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace Test;

internal class FieldBackedPropertyAttribute : Attribute
{
}

internal partial class Program
{
    [FieldBackedProperty]
    public int Test { get; }
}
").ConfigureAwait(false);
    }
}
