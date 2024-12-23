﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<CSL1000VariableshouldBeMadeConstant, CSL1000CodeFixProvider>;

[TestClass]
public partial class CSL1000UnitTests
{
    [TestMethod]
    public async Task LocalIntCouldBeConstant_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
class Program
{
    static void Main()
    {
        [|int i = 0;|]
        Console.WriteLine(i);
    }
}
", @"
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

    [TestMethod]
    public async Task VariableIsAssigned_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
class Program
{
    static void Main()
    {
        int i = 0;
        Console.WriteLine(i++);
    }
}
");
    }

    [TestMethod]
    public async Task VariableIsAlreadyConst_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
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

    [TestMethod]
    public async Task NoInitializer_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
class Program
{
    static void Main()
    {
        int i;
        i = 0;
        Console.WriteLine(i);
    }
}
");
    }

    [TestMethod]
    public async Task InitializerIsNotConstant_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
class Program
{
    static void Main()
    {
        int i = DateTime.Now.DayOfYear;
        Console.WriteLine(i);
    }
}
");
    }

    [TestMethod]
    public async Task MultipleInitializers_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
class Program
{
    static void Main()
    {
        int i = 0, j = DateTime.Now.DayOfYear;
        Console.WriteLine(i);
        Console.WriteLine(j);
    }
}
");
    }

    [TestMethod]
    public async Task DeclarationIsInvalid_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
class Program
{
    static void Main()
    {
        int x = {|CS0029:""abc""|};
    }
}
");
    }

    [TestMethod]
    public async Task DeclarationIsNotString_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
class Program
{
    static void Main()
    {
        object s = ""abc"";
    }
}
");
    }

    [TestMethod]
    public async Task ConstantIsNotNull_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
class Program
{
    static void Main()
    {
        object s = 0;
    }
}
");
    }

    [TestMethod]
    public async Task StringCouldBeConstant_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
class Program
{
    static void Main()
    {
        [|string s = ""abc"";|]
    }
}
", @"
class Program
{
    static void Main()
    {
        const string s = ""abc"";
    }
}
");
    }

    [TestMethod]
    public async Task VarIntDeclarationCouldBeConstant_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
class Program
{
    static void Main()
    {
        [|var item = 4;|]
    }
}
", @"
class Program
{
    static void Main()
    {
        const int item = 4;
    }
}
");
    }

    [TestMethod]
    public async Task VarStringDeclarationCouldBeConstant_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
class Program
{
    static void Main()
    {
        [|var item = ""abc"";|]
    }
}
", @"
class Program
{
    static void Main()
    {
        const string item = ""abc"";
    }
}
");
    }

    [TestMethod]
    public async Task NullStringCouldBeConstant_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
class Program
{
    static void Main()
    {
        [|string s = null;|]
    }
}
", @"
class Program
{
    static void Main()
    {
        const string s = null;
    }
}
");
    }

    [TestMethod]
    public async Task StringIsAliased_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.Default + @"
using var = System.String;
", @"
class Program
{
    static void Main()
    {
        [|var s = ""abc"";|]
    }
}
", @"
class Program
{
    static void Main()
    {
        const var s = ""abc"";
    }
}
");
    }

    [TestMethod]
    public async Task VarIsType_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
class Program
{
    static void Main()
    {
        [|var s = null;|]
    }
}

class var
{
}
", @"
class Program
{
    static void Main()
    {
        const var s = null;
    }
}

class var
{
}
");
    }
}
