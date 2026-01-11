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
    [GeneratedCode("CSharpLatest.Analyzers","2.6.1.40")]
    file event AsyncEventHandler<string, EventArgs> Foo
    {
        add => __foo.Register(value);
        remove => __foo.Unregister(value);
    }

    private readonly AsyncEventDispatcher<string, EventArgs> __foo = new();

    private async Task RaiseFoo(string? sender, EventArgs args, CancellationToken cancellationToken = default)
        => await __foo.InvokeAsync(sender, args, cancellationToken).ConfigureAwait(false);
}