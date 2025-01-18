#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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
    [DataRow("recursive;;")]
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
    [DataRow("recursive;;")]
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
    [DataRow("recursive;;")]
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

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
    [DataRow("never;;")]
    public async Task NestedIfNoElse_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
        {
            if (n > 1)
                return 2;
        }
        else
            return 0;

        return 1;
    }
}
");
    }

    [TestMethod]
    [DataRow("never;;")]
    public async Task NestedIfElse_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0)
        {
            if (n > 1)
                return 2;
            else
                return 1;
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
            if (n > 1)
                return 2;
            else
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
    [DataRow("recursive;;")]
    public async Task NestedIfElse_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
        {
            if (n > 1)
                return 2;
            else
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
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
    [DataRow("never;;")]
    public async Task MultiLineIfCondition_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n
                >
                0)
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
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
    [DataRow("never;;")]
    public async Task MultiLineWhileCondition_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        int i = 0;

        do
        {
            i++;
        }
        while (i
               <
               n);

        return i;
    }
}
");
    }

    [TestMethod]
    [DataRow("never;;")]
    public async Task SingleLineWhileCondition_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        int i = 0;

        [|do|]
        {
            i++;
        }
        while (i < n);

        return i;
    }
}
", @"
class Program
{
    public int Foo(int n)
    {
        int i = 0;

        do
            i++;
        while (i < n);

        return i;
    }
}
");
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
    public async Task SingleLineWhileCondition_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        int i = 0;

        do
        {
            i++;
        }
        while (i < n);

        return i;
    }
}
");
    }

    [TestMethod]
    [DataRow("never;;")]
    public async Task WhileWithMultipleLineStatement_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        int i = 0;

        [|do|]
        {
            if (n > 0)
                i++;
            else
            {
            }
        }
        while (i < n);

        return i;
    }
}
", @"
class Program
{
    public int Foo(int n)
    {
        int i = 0;

        do
            if (n > 0)
                i++;
            else
            {
            }
        while (i < n);

        return i;
    }
}
");
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
    public async Task WhileWithMultipleLineStatement_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        int i = 0;

        do
        {
            if (n > 0)
                i++;
            else
            {
            }
        }
        while (i < n);

        return i;
    }
}
");
    }

    [TestMethod]
    [DataRow("never;;")]
    public async Task RepeatedUsing_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.Default, @"
using System.IO;

class Program
{
    public void Foo()
    {
        using (FileStream source = new(@""C:\source"", FileMode.Open, FileAccess.Read))
        [|using|] (FileStream destination = new(@""C:\destination"", FileMode.Create, FileAccess.Write))
        {
            source.Flush();
        }
    }
}
", @"
using System.IO;

class Program
{
    public void Foo()
    {
        using (FileStream source = new(@""C:\source"", FileMode.Open, FileAccess.Read))
        using (FileStream destination = new(@""C:\destination"", FileMode.Create, FileAccess.Write))
            source.Flush();
    }
}
");
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
    public async Task RepeatedUsing_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.Default, @"
using System.IO;

class Program
{
    public void Foo()
    {
        using (FileStream source = new(@""C:\source"", FileMode.Open, FileAccess.Read))
        using (FileStream destination = new(@""C:\destination"", FileMode.Create, FileAccess.Write))
        {
            source.Flush();
        }
    }
}
");
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
    [DataRow("never;;")]
    public async Task IfWithTrivia_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
        {
            // Trivia
            return 1;
        }

        return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow("never;;")]
    public async Task IfMultiline_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0)
        {
            return
                1;
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
            return
                1;

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
    [DataRow("recursive;;")]
    public async Task IfMultiline_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
        {
            return
                1;
        }

        return 0;
    }
}
");
    }
}
