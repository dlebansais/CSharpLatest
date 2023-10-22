namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpLatest.Test.CSharpCodeFixVerifier<
    CSharpLatest.CSL1000VariableshouldBeMadeConstant,
    CSharpLatest.CSL1000CodeFixProvider>;

public partial class CSL1000UnitTests
{
    [TestMethod]
    public async Task CoverageDirective_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952

using System;

class Program
{
    static void Main()
    {
        int i = 0;
        Console.WriteLine(i);
    }
}
");
    }

    [TestMethod]
    public async Task InvalidDeclaration_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        var i = new();
        Console.WriteLine(i++);
    }
}
", DiagnosticResult.CompilerError("CS8754").WithSpan(8, 17, 8, 22).WithArguments("new()"));
    }
}
