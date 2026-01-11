//HintName: Program_FooAsync.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

using System;
using System.CodeDom.Compiler;
using System.Threading;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

partial record struct Program
{
    [GeneratedCode("CSharpLatest.Analyzers","2.6.1.40")]
    public event AsyncEventHandler Foo
    {
        add => __foo.Register(value);
        remove => __foo.Unregister(value);
    }

    private readonly AsyncEventDispatcher __foo = new();

    private async Task RaiseFoo(CancellationToken cancellationToken = default)
        => await __foo.InvokeAsync(this, cancellationToken).ConfigureAwait(false);
}