﻿namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<CSL1000VariableshouldBeMadeConstant, CSL1000CodeFixProvider>;

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
        var i = {|CS8754:new()|};
        Console.WriteLine(i++);
    }
}
");
    }
}
