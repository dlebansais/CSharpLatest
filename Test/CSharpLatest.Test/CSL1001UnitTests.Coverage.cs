namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<CSL1001UseIsNull, CSL1001CodeFixProvider>;

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
        if ({|CS0103:x|} == null)
            Console.WriteLine(string.Empty);
    }
}
");
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

        if ({|CS0019:x == null|})
            Console.WriteLine(string.Empty);
    }
}

struct Foo
{
}
");
    }
}
