//HintName: Program_FooAsync.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using CSharpLatest;

partial struct Program
{
    [GeneratedCode("CSharpLatest.Analyzers","2.6.2.41")]
    public void Foo()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await FooAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Fatal: exception in FooAsync.\r\n{exception.Message}\r\n{exception.StackTrace}");
                throw;
            }
        });
    }
}