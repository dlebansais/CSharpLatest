using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpLatest.Test.CSharpCodeFixVerifier<
    CSharpLatest.CSharpLatestAnalyzer,
    CSharpLatest.CSharpLatestCodeFixProvider>;

namespace CSharpLatest.Test;

[TestClass]
public class CSharpLatestUnitTest
{
    [TestMethod]
    public async Task LocalIntCouldBeConstant_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class Program
{
    static void Main()
    {
        [|int i = 0;|]
        Console.WriteLine(i);
    }
}
", @"
using System;

class Program
{
    static void Main()
    {
        const int i = 0;
        Console.WriteLine(i);
    }
}
");
    }
}
