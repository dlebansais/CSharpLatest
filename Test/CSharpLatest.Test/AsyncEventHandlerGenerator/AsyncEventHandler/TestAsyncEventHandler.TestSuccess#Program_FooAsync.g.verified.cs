//HintName: Program_FooAsync.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

partial class Program
{
    [GeneratedCodeAttribute("CSharpLatest.Analyzers","2.5.0.37")]
    public void Foo()
    {
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            await FooAsync().ConfigureAwait(false);
        });
    }
}