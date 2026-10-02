//HintName: Program_Test.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

using System.CodeDom.Compiler;

partial record Program
{
    [GeneratedCode("CSharpLatest.Analyzers","3.1.0.50")]
    public partial int Test
    {
        get => fieldTest;
        set => fieldTest = value;
    }

    private int fieldTest = 0;
}