//HintName: Program_Test.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

partial class Program
{
    /// <summary>
    /// Test doc.
    /// </summary>
    /// <param name="value">The property value.</param>
    /// <returns>The getter.</returns>
    [GeneratedCodeAttribute("CSharpLatest.Analyzers","2.2.0.29")]
    public partial int Test
    {
        get => fieldTest;
        set => fieldTest = value;
    }

    private int fieldTest = 0;
}