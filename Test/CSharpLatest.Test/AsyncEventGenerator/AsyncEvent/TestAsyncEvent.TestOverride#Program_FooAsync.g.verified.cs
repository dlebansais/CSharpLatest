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
    [GeneratedCode("CSharpLatest.Analyzers","2.6.2.41")]
    public override event AsyncEventHandler Foo
    {
        add => __foo.Register(value);
        remove => __foo.Unregister(value);
    }

    private readonly AsyncEventDispatcher __foo = new();

    private async Task RaiseFoo(CancellationToken cancellationToken = default)
        => await __foo.InvokeAsync(this, cancellationToken).ConfigureAwait(false);
}