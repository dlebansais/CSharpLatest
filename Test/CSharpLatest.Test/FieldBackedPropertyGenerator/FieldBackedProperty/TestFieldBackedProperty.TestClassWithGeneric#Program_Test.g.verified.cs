//HintName: Program_Test.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

using System.CodeDom.Compiler;

partial class Program<T>
{
    [GeneratedCode("CSharpLatest.Analyzers","2.6.4.43")]
    public partial int Test
    {
        get => fieldTest;
        set => fieldTest = value;
    }

    private int fieldTest = 0;
}