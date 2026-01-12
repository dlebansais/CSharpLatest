//HintName: Program_FooAsync.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

using global::System.CodeDom.Compiler;
using global::System.Diagnostics;
using global::System.Threading.Tasks;
using CSharpLatest;

partial class Program
{
    [GeneratedCode("CSharpLatest.Analyzers","2.6.3.42")]
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