#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1007AddMissingBraces, CSL1007CodeFixProvider>;

[TestClass]
internal partial class CSL1007UnitTests
{
    [TestMethod]
    [DataRow(";false;")]
    [DataRow("true;false;")]
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    public async Task InvalidEmbeddedStatement_NoDiagnostic1(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        DiagnosticDescriptor DescriptorCS0161 = new(
            "CS0161",
            "title",
            "'Program.Foo(int)': not all code paths return a value",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticDescriptor DescriptorCSL1007 = new(
            "CSL1007",
            "title",
            "Add missing braces to 'if' statement",
            "description",
            DiagnosticSeverity.Warning,
            true
            );

        DiagnosticDescriptor DescriptorCS1002 = new(
            "CS1002",
            "title",
            "; expected",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticDescriptor DescriptorCS1525 = new(
            "CS1525",
            "title",
            "Invalid expression term '}'",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticDescriptor DescriptorCS1519 = new(
            "CS1519",
            "title",
            "Invalid token 'return' in a member declaration",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticDescriptor DescriptorCS1022 = new(
            "CS1022",
            "title",
            "Type or namespace definition, or end-of-file expected",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected1 = new(DescriptorCS0161);
        Expected1 = Expected1.WithLocation("/0/Test0.cs", 17, 16);

        DiagnosticResult Expected2 = new(DescriptorCSL1007);
        Expected2 = Expected2.WithLocation("/0/Test0.cs", 19, 9);

        DiagnosticResult Expected3 = new(DescriptorCS1002);
        Expected3 = Expected3.WithLocation("/0/Test0.cs", 19, 19);

        DiagnosticResult Expected4 = new(DescriptorCS1525);
        Expected4 = Expected4.WithLocation("/0/Test0.cs", 19, 19);

        DiagnosticResult Expected5 = new(DescriptorCS1519);
        Expected5 = Expected5.WithLocation("/0/Test0.cs", 22, 9);

        DiagnosticResult Expected6 = new(DescriptorCS1022);
        Expected6 = Expected6.WithLocation("/0/Test0.cs", 24, 1);

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
Expected6).ConfigureAwait(false);
    }

    [TestMethod]
    [DataRow("false;;")]
    [DataRow("when_multiline;;")]
    [DataRow("never;;")]
    [DataRow("recursive;;")]
    public async Task InvalidEmbeddedStatement_NoDiagnostic2(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        DiagnosticDescriptor DescriptorCS0161 = new(
            "CS0161",
            "title",
            "'Program.Foo(int)': not all code paths return a value",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticDescriptor DescriptorCS1002 = new(
            "CS1002",
            "title",
            "; expected",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticDescriptor DescriptorCS1525 = new(
            "CS1525",
            "title",
            "Invalid expression term '}'",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticDescriptor DescriptorCS1519 = new(
            "CS1519",
            "title",
            "Invalid token 'return' in a member declaration",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticDescriptor DescriptorCS1022 = new(
            "CS1022",
            "title",
            "Type or namespace definition, or end-of-file expected",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected1 = new(DescriptorCS0161);
        Expected1 = Expected1.WithLocation("/0/Test0.cs", 17, 16);

        DiagnosticResult Expected2 = new(DescriptorCS1002);
        Expected2 = Expected2.WithLocation("/0/Test0.cs", 19, 19);

        DiagnosticResult Expected3 = new(DescriptorCS1525);
        Expected3 = Expected3.WithLocation("/0/Test0.cs", 19, 19);

        DiagnosticResult Expected4 = new(DescriptorCS1519);
        Expected4 = Expected4.WithLocation("/0/Test0.cs", 22, 9);

        DiagnosticResult Expected5 = new(DescriptorCS1022);
        Expected5 = Expected5.WithLocation("/0/Test0.cs", 24, 1);

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
Expected5).ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
    }

    [TestMethod]
    [DataRow(";;")]
    [DataRow("true;;")]
    [DataRow("when_multiline;;")]
    [DataRow("recursive;;")]
    public async Task IfWithMultilineInstruction_Diagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
class Program
{
    public int Foo(int n)
    {
        [|if|] (n > 0)
            return
                1;

        return 0;
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    [DataRow("false;;")]
    [DataRow("never;;")]
    public async Task IfWithMultilineInstruction_NoDiagnostic(string args)
    {
        Dictionary<string, string> Options = TestTools.ToOptions(args);

        await VerifyCS.VerifyAnalyzerAsync(Options, Prologs.IsExternalInit, @"
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
").ConfigureAwait(false);
    }
}
