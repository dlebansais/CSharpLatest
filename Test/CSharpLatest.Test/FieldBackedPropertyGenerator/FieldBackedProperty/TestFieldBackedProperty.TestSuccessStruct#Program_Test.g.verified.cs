//HintName: Program_Test.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

using System.CodeDom.Compiler;

partial struct Program
{
    [GeneratedCode("CSharpLatest.Analyzers","2.6.3.42")]
    public partial int Test
    {
        get => fieldTest;
        set => fieldTest = value;
    }

    private int fieldTest = 0;
}