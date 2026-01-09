//HintName: Program_FooAsync.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

using System.Diagnostics;

partial class Program
{
    [GeneratedCodeAttribute("CSharpLatest.Analyzers","2.5.0.37")]
    public sealed void Foo()
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