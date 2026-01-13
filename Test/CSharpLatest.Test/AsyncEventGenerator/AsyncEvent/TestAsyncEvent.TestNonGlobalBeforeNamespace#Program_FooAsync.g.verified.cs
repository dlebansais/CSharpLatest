//HintName: Program_FooAsync.g.cs
#nullable enable

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

namespace CSharpLatest.TestSuite;

using System.CodeDom.Compiler;
using System.Threading;

partial class Program
{
    [GeneratedCode("CSharpLatest.Analyzers","2.6.4.43")]
    public partial event AsyncEventHandler Foo
    {
        add => __foo.Register(value);
        remove => __foo.Unregister(value);
    }

    private readonly AsyncEventDispatcher __foo = new();

    private async Task RaiseFoo(CancellationToken cancellationToken = default)
        => await __foo.InvokeAsync(this, cancellationToken).ConfigureAwait(false);
}