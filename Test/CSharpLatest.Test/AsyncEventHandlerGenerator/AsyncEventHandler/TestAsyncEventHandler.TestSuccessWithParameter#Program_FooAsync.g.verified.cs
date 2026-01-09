//HintName: Program_FooAsync.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

using System.Diagnostics;

partial class Program
{
    [GeneratedCodeAttribute("CSharpLatest.Analyzers","2.5.1.38")]
    public void Foo(object? sender, EventArgs args)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await FooAsync(sender, args).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Fatal: exception in FooAsync.\r\n{exception.Message}\r\n{exception.StackTrace}");
                throw;
            }
        });
    }
}