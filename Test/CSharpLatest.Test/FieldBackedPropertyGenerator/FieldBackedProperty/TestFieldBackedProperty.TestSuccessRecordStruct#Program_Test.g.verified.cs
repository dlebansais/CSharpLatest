//HintName: Program_Test.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

partial record struct Program
{
    [GeneratedCodeAttribute("CSharpLatest.Analyzers","2.5.1.38")]
    public partial int Test
    {
        get => fieldTest;
        set => fieldTest = value;
    }

    private int fieldTest = 0;
}