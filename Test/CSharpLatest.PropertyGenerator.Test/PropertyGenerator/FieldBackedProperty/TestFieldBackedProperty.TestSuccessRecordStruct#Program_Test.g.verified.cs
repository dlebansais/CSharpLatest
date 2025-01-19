//HintName: Program_Test.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

partial record struct Program
{
    [GeneratedCodeAttribute("CSharpLatest.Analyzers","2.1.1.23")]
    public partial int Test
    {
        get => fieldTest;
        set => fieldTest = value;
    }

    private int fieldTest = 0;
}