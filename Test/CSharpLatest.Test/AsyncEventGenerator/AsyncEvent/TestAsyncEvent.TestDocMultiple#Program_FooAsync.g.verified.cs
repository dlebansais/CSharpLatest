//HintName: Program_FooAsync.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

using System;
using System.CodeDom.Compiler;
using System.Threading;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

partial class Program
{
    /// <summary>
    /// Test doc.
    /// </summary>

    /// <param name="value">The property value.</param>

    /// <returns>The getter.</returns>
    [GeneratedCode("CSharpLatest.Analyzers","2.6.3.42")]
    public partial event AsyncEventHandler Foo
    {
        add => __foo.Register(value);
        remove => __foo.Unregister(value);
    }

    private readonly AsyncEventDispatcher __foo = new();

    private async Task RaiseFoo(CancellationToken cancellationToken = default)
        => await __foo.InvokeAsync(this, cancellationToken).ConfigureAwait(false);
}