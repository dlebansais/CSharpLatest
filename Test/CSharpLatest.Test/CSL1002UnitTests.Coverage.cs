namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpLatest.Test.CSharpCodeFixVerifier<
    CSharpLatest.CSL1002UseIsNotNull,
    CSharpLatest.CSL1002CodeFixProvider>;

public partial class CSL1002UnitTests
{
    [TestMethod]
    public async Task NotDifferentThanLiteral_NoDiagnostic()
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

        if (s1 != s2)
            Console.WriteLine(string.Empty);
    }
}
");
    }

    [TestMethod]
    public async Task NotDifferentThanNull_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if (s != ""test"")
            Console.WriteLine(string.Empty);
    }
}
");
    }
}
