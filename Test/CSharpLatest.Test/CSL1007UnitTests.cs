namespace CSharpLatest.Test;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
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
    [DataRow("recursive;;")]
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
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
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
    [DataRow("recursive;;")]
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

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
    public async Task IfElseMultiLineIfElse_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0)
            return 1;
        else if (n > 1)
        {
            return 2;
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
    public async Task IfElseMultiLineIfElse_NoDiagnostic(string args)
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
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
    public async Task IfElseIfMultiLineElse_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0)
            return 1;
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
    public async Task IfElseIfMultiLineElse_NoDiagnostic(string args)
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
            return 2;
        else
        {
            return 0;
        }
    }
}
");
    }

    [TestMethod]
    [DataRow("when_multiline;;")]
    public async Task NestedOneLineIf_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0)
            if ((n % 2) == 0)
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
            if ((n % 2) == 0)
                return 1;
        }

        return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow("false;;")]
    [DataRow("never;;")]
    [DataRow("recursive;;")]
    public async Task NestedOneLineIf_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
            if ((n % 2) == 0)
                return 1;

        return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow("when_multiline;;")]
    public async Task NestedOneLineElse_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0)
            if ((n % 2) == 0)
                return 2;
            else
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
            if ((n % 2) == 0)
                return 2;
            else
                return 1;
        }

        return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow("false;;")]
    [DataRow("never;;")]
    [DataRow("recursive;;")]
    public async Task NestedOneLineElse_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
            if ((n % 2) == 0)
                return 2;
            else
                return 1;

        return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow("when_multiline;;")]
    public async Task NestedOneLineIfAndElse_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        // Verifiy the diagnostic, but not the code fix.
        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0)
            if ((n % 2) == 0)
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
    [DataRow("false;;")]
    [DataRow("never;;")]
    [DataRow("recursive;;")]
    public async Task NestedOneLineIfAndElse_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
            if ((n % 2) == 0)
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
    [DataRow("when_multiline;;")]
    public async Task MultiNestedOneLineIf_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0)
            [|if|] ((n % 2) == 0)
                if ((n % 3) == 0)
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
            if ((n % 2) == 0)
            {
                if ((n % 3) == 0)
                    return 1;
            }
        }

        return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow("false;;")]
    [DataRow("never;;")]
    [DataRow("recursive;;")]
    public async Task MultiNestedOneLineIf_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
            if ((n % 2) == 0)
                if ((n % 3) == 0)
                    return 1;

        return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    public async Task InvalidEmbeddedStatement_NoDiagnostic1(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        var DescriptorCS0161 = new DiagnosticDescriptor(
            "CS0161",
            "title",
            "'Program.Foo(int)': not all code paths return a value",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        var DescriptorCSL1007 = new DiagnosticDescriptor(
            "CSL1007",
            "title",
            "Add missing braces to 'if' statement",
            "description",
            DiagnosticSeverity.Warning,
            true
            );

        var DescriptorCS1002 = new DiagnosticDescriptor(
            "CS1002",
            "title",
            "; expected",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        var DescriptorCS1525 = new DiagnosticDescriptor(
            "CS1525",
            "title",
            "Invalid expression term '}'",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        var DescriptorCS1519 = new DiagnosticDescriptor(
            "CS1519",
            "title",
            "Invalid token 'return' in class, record, struct, or interface member declaration",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        var DescriptorCS1022 = new DiagnosticDescriptor(
            "CS1022",
            "title",
            "Type or namespace definition, or end-of-file expected",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        var Expected1 = new DiagnosticResult(DescriptorCS0161);
        Expected1 = Expected1.WithLocation("/0/Test0.cs", 15, 16);

        var Expected2 = new DiagnosticResult(DescriptorCSL1007);
        Expected2 = Expected2.WithLocation("/0/Test0.cs", 17, 9);

        var Expected3 = new DiagnosticResult(DescriptorCS1002);
        Expected3 = Expected3.WithLocation("/0/Test0.cs", 17, 19);

        var Expected4 = new DiagnosticResult(DescriptorCS1525);
        Expected4 = Expected4.WithLocation("/0/Test0.cs", 17, 19);

        var Expected5 = new DiagnosticResult(DescriptorCS1519);
        Expected5 = Expected5.WithLocation("/0/Test0.cs", 20, 9);

        var Expected6 = new DiagnosticResult(DescriptorCS1022);
        Expected6 = Expected6.WithLocation("/0/Test0.cs", 22, 1);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
            }

        return 0;
    }
}
", LanguageVersion.Default,
Expected1,
Expected2,
Expected3,
Expected4,
Expected5,
Expected6);
    }

    [TestMethod]
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    [DataRow("never;;")]
    [DataRow("recursive;;")]
    public async Task InvalidEmbeddedStatement_NoDiagnostic2(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        var DescriptorCS0161 = new DiagnosticDescriptor(
            "CS0161",
            "title",
            "'Program.Foo(int)': not all code paths return a value",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        var DescriptorCS1002 = new DiagnosticDescriptor(
            "CS1002",
            "title",
            "; expected",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        var DescriptorCS1525 = new DiagnosticDescriptor(
            "CS1525",
            "title",
            "Invalid expression term '}'",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        var DescriptorCS1519 = new DiagnosticDescriptor(
            "CS1519",
            "title",
            "Invalid token 'return' in class, record, struct, or interface member declaration",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        var DescriptorCS1022 = new DiagnosticDescriptor(
            "CS1022",
            "title",
            "Type or namespace definition, or end-of-file expected",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        var Expected1 = new DiagnosticResult(DescriptorCS0161);
        Expected1 = Expected1.WithLocation("/0/Test0.cs", 15, 16);

        var Expected2 = new DiagnosticResult(DescriptorCS1002);
        Expected2 = Expected2.WithLocation("/0/Test0.cs", 17, 19);

        var Expected3 = new DiagnosticResult(DescriptorCS1525);
        Expected3 = Expected3.WithLocation("/0/Test0.cs", 17, 19);

        var Expected4 = new DiagnosticResult(DescriptorCS1519);
        Expected4 = Expected4.WithLocation("/0/Test0.cs", 20, 9);

        var Expected5 = new DiagnosticResult(DescriptorCS1022);
        Expected5 = Expected5.WithLocation("/0/Test0.cs", 22, 1);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0)
            }

        return 0;
    }
}
", LanguageVersion.Default,
Expected1,
Expected2,
Expected3,
Expected4,
Expected5);
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    public async Task SingleLineIfStatement_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0) return 1;

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
    [DataRow("recursive;;")]
    public async Task SingleLineIfStatement_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        if (n > 0) return 1;

        return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
    public async Task MultiLineIfCondition_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n
                >
                0) return 1;

        return 0;
    }
}
", @"
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
    [DataRow("false;;")]
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
                0) return 1;

        return 0;
    }
}
");
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
    public async Task MultiLineWhileCondition_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyCodeFixAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        int i = 0;

        [|do|]
            i++;
        while (i
               <
               n);

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
    [DataRow("false;;")]
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
            i++;
        while (i
               <
               n);

        return i;
    }
}
");
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
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
            i++;
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
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
    [DataRow("false;;")]
    [DataRow("never;;")]
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
    [DataRow("never;;")]
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
        }
    }
}
");
    }
}
