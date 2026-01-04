#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1015DoNotDeclareAsyncVoidMethods, CSL1015CodeFixProvider>;

[TestClass]
internal partial class CSL1015UnitTests
{
    [TestMethod]
    public async Task AsyncVoidReturnType_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.Default, @"
class Program
{
    [|async void Foo()
    {
    }|]
}
", @"
class Program
{
    async Task Foo()
    {
    }
}
", LanguageVersion.CSharp5).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NotAsyncReturnType_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
class Program
{
    void Foo()
    {
    }
}
", LanguageVersion.CSharp5).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NotVoidReturnType_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
class Program
{
    async Task Foo()
    {
    }
}
", LanguageVersion.CSharp5).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoUsing_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync("", @"
class Program
{
    [|async void Foo()
    {
    }|]
}
", @"using System.Threading.Tasks;

class Program
{
    async Task Foo()
    {
    }
}
", LanguageVersion.CSharp5).ConfigureAwait(false);
    }
}
