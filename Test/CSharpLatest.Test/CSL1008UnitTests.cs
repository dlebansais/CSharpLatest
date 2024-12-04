namespace CSharpLatest.Test;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<CSL1008RemoveUnnecessaryBraces, CSL1008CodeFixProvider>;

[TestClass]
public partial class CSL1008UnitTests
{
    [TestMethod]
    [DataRow("never;;")]
    public async Task IfWithBraces_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0)
        {
            return 1;
        }

        return 0;
    }
}
", @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
            return 1;

        return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    public async Task IfWithBraces_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
        {
            return 1;
        }

        return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow("never;;")]
    public async Task OneLineIfAndElse_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0)
        {
            return 1;
        }
        [|else|]
        {
            return 0;
        }
    }
}
", @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
            return 1;
        else
            return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    public async Task OneLineIfAndElse_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
");
    }

    [TestMethod]
    [DataRow("never;;")]
    public async Task OneLineIfElseIfElse_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
            return 1;
        else [|if|] (n > 1)
        {
            return 2;
        }
        else
            return 0;
    }
}
", @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
            return 1;
        else if (n > 1)
            return 2;
        else
            return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    public async Task OneLineIfElseIfElse_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
            return 1;
        else if (n > 1)
        {
            return 2;
        }
        else
            return 0;
    }
}
");
    }
}
