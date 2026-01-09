#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<Analyzers::CSharpLatest.CSL1016UnsupportedUseOfTheAsyncEventHandlerAttribute>;

[TestClass]
internal partial class CSL1016UnitTests
{
    [TestMethod]
    public async Task NotMethod_Diagnostic()
    {
        DiagnosticDescriptor DescriptorCS0592 = new(
            "CS0592",
            "title",
            "Attribute 'AsyncEventHandler' is not valid on this declaration type. It is only valid on 'method' declarations.",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected = new(DescriptorCS0592);
        Expected = Expected.WithLocation("/0/Test0.cs", 11, 6);

        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal partial class Program
{
    [[|AsyncEventHandler|]]
    public int Test { get; set; }
}
", LanguageVersion.CSharp10, FrameworkChoice.Default, Expected).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoClass_Diagnostic()
    {
        DiagnosticDescriptor DescriptorCS0116 = new(
            "CS0116",
            "title",
            "A namespace cannot directly contain members such as fields, methods or statements",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected = new(DescriptorCS0116);
        Expected = Expected.WithLocation("/0/Test0.cs", 10, 19);

        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

[[|AsyncEventHandler|]]
public async Task FooAsync()
{
    await Task.Delay(0);
}
", LanguageVersion.CSharp10, FrameworkChoice.Default, Expected).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoNamespace1_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
internal partial class Program
{
    [[|AsyncEventHandler|]]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoNamespace2_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
internal partial struct Program
{
    [[|AsyncEventHandler|]]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoNamespace3_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
internal partial record Program
{
    [[|AsyncEventHandler|]]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoNamespace4_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
internal partial record struct Program
{
    [[|AsyncEventHandler|]]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
").ConfigureAwait(false);
    }

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

    [TestMethod]
    public async Task MethodNotAsync_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal partial class Program
{
    [[|AsyncEventHandler|]]
    public Task FooAsync()
    {
        return Task.Delay(0);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task MethodNotTask1_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal partial class Program
{
    [[|AsyncEventHandler|]]
    public async void FooAsync()
    {
        await Task.Delay(0);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task MethodNotTask2_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal partial class Program
{
    [[|AsyncEventHandler|]]
    public async Task<int> FooAsync()
    {
        return await Task.FromResult(0);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task MethodNotTask3_Diagnostic()
    {
        DiagnosticDescriptor DescriptorCS1983 = new(
            "CS1983",
            "title",
            "The return type of an async method must be void, Task, Task<T>, a task-like type, IAsyncEnumerable<T>, or IAsyncEnumerator<T>",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected = new(DescriptorCS1983);
        Expected = Expected.WithLocation("/0/Test0.cs", 12, 26);

        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal partial class Program
{
    [[|AsyncEventHandler|]]
    public async Program FooAsync()
    {
        return this;
    }
}
", LanguageVersion.CSharp10, FrameworkChoice.Default, Expected).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task MethodNotAsyncSuffix_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal partial class Program
{
    [[|AsyncEventHandler|]]
    public async Task FooAsynchronous()
    {
        await Task.Delay(0);
    }
}
").ConfigureAwait(false);
    }
}
