﻿namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpLatest.Test.CSharpCodeFixVerifier<
    CSharpLatest.CSL1001UseIsNull,
    CSharpLatest.CSL1001CodeFixProvider>;

public partial class CSL1001UnitTests
{
    [TestMethod]
    public async Task CoverageDirective_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952
#nullable enable

using System;

class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if (s == null)
            Console.WriteLine(string.Empty);
    }
}
");
    }

    [TestMethod]
    public async Task NotEqualToNull_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class Program
{
    static void Main(string[] args)
    {
        string? s1 = args.Length > 0 ? null : ""test"";
        string? s2 = args.Length > 0 ? null : ""test"";

        if (s1 == s2)
            Console.WriteLine(string.Empty);
    }
}
");
    }

    [TestMethod]
    public async Task NotEqualToLiteral_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if (s == ""test"")
            Console.WriteLine(string.Empty);
    }
}
");
    }

    [TestMethod]
    public async Task UnknownEqualsEqualsOperator_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class Program
{
    static void Main(string[] args)
    {
        if (x == null)
            Console.WriteLine(string.Empty);
    }
}
", DiagnosticResult.CompilerError("CS0103").WithSpan(10, 13, 10, 14).WithArguments("x"));
    }

    [TestMethod]
    public async Task StructEqualsEqualsOperator_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class Program
{
    static void Main(string[] args)
    {
        Foo x;

        if (x == null)
            Console.WriteLine(string.Empty);
    }
}

struct Foo
{
}
", DiagnosticResult.CompilerError("CS0019").WithSpan(12, 13, 12, 22).WithArguments("==", "Foo", "<null>"));
    }
}