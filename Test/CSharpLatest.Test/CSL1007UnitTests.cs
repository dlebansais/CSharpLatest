namespace CSharpLatest.Test;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<CSL1007AddMissingBraces, CSL1007CodeFixProvider>;

[TestClass]
public partial class CSL1007UnitTests
{
    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    public async Task OneLineIf_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0)
            return 1;

        return 0;
    }
}
", @"
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
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    [DataRow("never;;")]
    public async Task OneLineIf_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
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
    [DataRow("never;;")]
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
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("when_multiline;;")]
    public async Task OneLineElse_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
        {
            return 1;
        }
        [|else|]
            return 0;
    }
}
", @"
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
    [DataRow("false;;")]
    [DataRow("never;;")]
    public async Task OneLineElse_NoDiagnostic(string args)
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
            return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    public async Task OneLineIfAndElse_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0)
            return 1;
        [|else|]
            return 0;
    }
}
", @"
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
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    [DataRow("never;;")]
    public async Task OneLineIfAndElse_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
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
    [DataRow("when_multiline;;")]
    public async Task OneLineIfElseIfElse_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
        {
            return 1;
        }
        else [|if|] (n > 1)
            return 2;
        else
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
        {
            return 1;
        }
        else if (n > 1)
        {
            return 2;
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
    [DataRow("false;;")]
    [DataRow("never;;")]
    public async Task OneLineIfElseIfElse_NoDiagnostic(string args)
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
        else if (n > 1)
            return 2;
        else
        {
            return 0;
        }
    }
}
");
    }
}
