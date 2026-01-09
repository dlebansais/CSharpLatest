#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<Analyzers::CSharpLatest.CSL1016HandlerAttributeIsMissingArgument>;

[TestClass]
internal partial class CSL1016UnitTests
{
    [TestMethod]
    public async Task EmptyArgument_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal partial class Program
{
    [[|AsyncEventHandler()|]]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoArgument_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
namespace TestSuite;

internal partial class Program
{
    [AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
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
    [AsyncEventHandler(UseDispatcher = false)]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
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
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
    public int Test { get; }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoArgumentOtherProperty_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace Test;

internal class AsyncEventHandlerAttribute : Attribute
{
}

internal partial class Program
{
    [AsyncEventHandler()]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
").ConfigureAwait(false);
    }
}
