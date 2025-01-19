#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1000VariableshouldBeMadeConstant, CSL1000CodeFixProvider>;

[TestClass]
internal partial class CSL1000UnitTests
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
", Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp4).ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoInitializerWithAssignment_NoDiagnostic()
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
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoInitializerNoAssignment_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
class Program
{
    static void Main()
    {
        int i;
        Console.WriteLine({|CS0165:i|});
    }
}
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task UserDefinedConversion_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"

class MyString
{
    private string s;
    public static implicit operator string(MyString myString) => myString.s;
    public static implicit operator MyString(string s) => new MyString { s = s };
}

class Program
{
    static void Main()
    {
        MyString x = ""x"";
    }
}
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
    }
}
