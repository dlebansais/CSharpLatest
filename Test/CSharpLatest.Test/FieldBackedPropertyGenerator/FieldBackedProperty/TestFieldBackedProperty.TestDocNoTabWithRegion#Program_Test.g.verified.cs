//HintName: Program_Test.g.cs
#nullable enable

namespace CSharpLatest.TestSuite;

using System.CodeDom.Compiler;

partial class Program
{
/// <summary>
/// Test doc.
/// </summary>
/// <param name="value">The property value.</param>
/// <returns>The getter.</returns>
    [GeneratedCode("CSharpLatest.Analyzers","2.6.0.39")]
    public partial int Test
    {
        get => fieldTest;
        set => fieldTest = value;
    }

    private int fieldTest = 0;
}