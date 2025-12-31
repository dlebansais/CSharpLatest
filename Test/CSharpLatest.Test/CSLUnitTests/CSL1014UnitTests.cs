#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<Analyzers::CSharpLatest.CSL1014ConsiderUsingInheritDoc>;

[TestClass]
internal partial class CSL1014UnitTests
{
    [TestMethod]
    public async Task MethodExistingDoc_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal partial class Program : IDisposable
{
    [|/// <summary>
    /// Disposes resources.
    /// </summary>|]
    public void Dispose()
    {
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task MethodUsingInheritDoc_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
namespace TestSuite;

internal partial class Program : IDisposable
{
    /// <inheritdoc />
    public void Dispose()
    {
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task PropertyExistingDoc_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal interface IDigit
{
    /// <summary>
    /// Gets the digit value.
    /// </summary>
    int Value { get; }
}

internal class Digit : IDigit
{
    [|/// <summary>
    /// Gets the digit value.
    /// </summary>|]
    public int Value { get; }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task PropertyUsingInheritDoc_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
namespace TestSuite;

internal interface IDigit
{
    /// <summary>
    /// Gets the digit value.
    /// </summary>
    int Value { get; }
}

internal class Digit : IDigit
{
    /// <inheritdoc />
    public int Value { get; }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task IndexerExistingDoc_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal class BaseDigit
{
    /// <summary>
    /// Gets the digit value.
    /// </summary>
    public virtual int this[int index] => index;
}

internal class Digit : BaseDigit
{
    [|/// <summary>
    /// Gets the digit value.
    /// </summary>|]
    public override int this[int index] => index;
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task IndexerUsingInheritDoc_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
namespace TestSuite;

internal class BaseDigit
{
    /// <summary>
    /// Gets the digit value.
    /// </summary>
    public virtual int this[int index] => index;
}

internal class Digit : BaseDigit
{
    /// <inheritdoc />
    public override int this[int index] => index;
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task FieldEventExistingDoc_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal interface IDigit
{
    /// <summary>
    /// Event raised when threshold is reached.
    /// </summary>
    event System.EventHandler? ThresholdReached;
}

internal class Digit : IDigit
{
    [|/// <summary>
    /// Event raised when threshold is reached.
    /// </summary>|]
    public event System.EventHandler? ThresholdReached;
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task FieldEventUsingInheritDoc_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
namespace TestSuite;

internal interface IDigit
{
    /// <summary>
    /// Event raised when threshold is reached.
    /// </summary>
    event System.EventHandler? ThresholdReached;
}

internal class Digit : IDigit
{
    /// <inheritdoc />
    public event System.EventHandler? ThresholdReached;
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task MultipleFieldEvents_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
namespace TestSuite;

internal interface IDigit
{
    /// <summary>
    /// Event raised when threshold is reached.
    /// </summary>
    event System.EventHandler? ThresholdReached, ThresholdReached2;
}

internal class Digit : IDigit
{
    /// <summary>
    /// Event raised when threshold is reached.
    /// </summary>
    public event System.EventHandler? ThresholdReached, ThresholdReached2;
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task PropertyEventExistingDoc_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal class BaseDigit
{
    /// <summary>
    /// Event raised when threshold is reached.
    /// </summary>
    public virtual event System.EventHandler? ThresholdReached
    {
        add => eventList.Add(value);
        remove => eventList.Remove(value);
    }

    private readonly System.Collections.Generic.List<System.EventHandler> eventList = [];
}

internal class Digit : BaseDigit
{
    [|/// <summary>
    /// Event raised when threshold is reached.
    /// </summary>|]
    public override event System.EventHandler? ThresholdReached
    {
        add => eventList.Add(value);
        remove => eventList.Remove(value);
    }

    private readonly System.Collections.Generic.List<System.EventHandler> eventList = [];
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task PropertyEventUsingInheritDoc_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
namespace TestSuite;

internal class BaseDigit
{
    /// <summary>
    /// Event raised when threshold is reached.
    /// </summary>
    public virtual event System.EventHandler? ThresholdReached
    {
        add => eventList.Add(value);
        remove => eventList.Remove(value);
    }

    private readonly System.Collections.Generic.List<System.EventHandler> eventList = [];
}

internal class Digit : BaseDigit
{
    /// <inheritdoc />
    public override event System.EventHandler? ThresholdReached
    {
        add => eventList.Add(value);
        remove => eventList.Remove(value);
    }

    private readonly System.Collections.Generic.List<System.EventHandler> eventList = [];
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoDoc_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
namespace TestSuite;

internal partial class Program : IDisposable
{
    public void Dispose()
    {
    }
}
").ConfigureAwait(false);
    }
}
