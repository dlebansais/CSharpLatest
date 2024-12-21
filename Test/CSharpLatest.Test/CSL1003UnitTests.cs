#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<CSL1003ConsiderUsingPrimaryConstructor, CSL1003CodeFixProvider>;

[TestClass]
public partial class CSL1003UnitTests
{
    [TestMethod]
    public async Task Replaced_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program(string prop)
{
    public string Prop { get; } = prop;
}
");
    }

    [TestMethod]
    public async Task NoParameterCandidate_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public Program(int prop)
    {
        Prop = prop.ToString();
    }

    public string Prop { get; }
}
");
    }

    [TestMethod]
    public async Task MultipleConstructors_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
    class [|Program|]
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public Program(string prop, int other)
        {
            Prop = prop;
        }

        public string Prop { get; }
    }
", @"
class Program(string prop)
{
    public Program(string prop, int other) : this(prop)
    {
    }

    public string Prop { get; } = prop;
}
");
    }

    [TestMethod]
    public async Task MultipleConstructorsWithExtraMethod_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
    class [|Program|]
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public Program(string prop, int other)
        {
            Prop = prop;
        }

        public string Prop { get; }

        private void Method()
        {
        }
    }
", @"
class Program(string prop)
{
    public Program(string prop, int other) : this(prop)
    {
    }

    public string Prop { get; } = prop;

    private void Method()
    {
    }
}
");
    }

    [TestMethod]
    public async Task MultipleConstructorsWithExtraProperty_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
    class [|Program|]
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public Program(string prop, int other)
        {
            Prop = prop;
        }

        public string Prop { get; }

        public int Test { get; }
    }
", @"
class Program(string prop)
{
    public Program(string prop, int other) : this(prop)
    {
    }

    public string Prop { get; } = prop;

    public int Test { get; }
}
");
    }

    [TestMethod]
    public async Task Decoration1_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
class [|Program|]
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public Program(string prop, int other)/**/
    {
        Prop = prop;
    }

    public string Prop { get; }
}
", @"
class Program(string prop)
{
    public Program(string prop, int other) : this(prop)/**/
    {
    }

    public string Prop { get; } = prop;
}
");
    }

    [TestMethod]
    public async Task Decoration2_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
class [|Program|]
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public Program(string prop, int other) => Prop = prop;/**/

    public string Prop { get; }
}
", @"
class Program(string prop)
{
    public Program(string prop, int other) : this(prop) { }/**/

    public string Prop { get; } = prop;
}
");
    }

    [TestMethod]
    public async Task MissingAssignment_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program
{
    public Program(string prop, string other)
    {
        Prop = prop;
        Other = other;
    }

    public Program(string prop, string other, int more)
    {
        Prop = prop;
    }

    public string? Prop { get; }
    public string? Other { get; }
}
");
    }

    [TestMethod]
    public async Task MultipleConstructorsExpressionBody_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
class [|Program|]
{
    public Program(string prop) => Prop = prop;

    public Program(string prop, int other)
    {
        Prop = prop;
    }

    public string Prop { get; }
}
", @"
class Program(string prop)
{
    public Program(string prop, int other) : this(prop)
    {
    }

    public string Prop { get; } = prop;
}
");
    }

    [TestMethod]
    public async Task MissingAssignmentExpressionBody_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program
{
    public Program(string prop, string other)
    {
        Prop = prop;
        Other = other;
    }

    public Program(string prop, string other, int more) => Foo();

    private void Foo()
    {
    }

    public string? Prop { get; }
    public string? Other { get; }
}
");
    }

    [TestMethod]
    public async Task ComplexExpression1_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program
{
    public Program(string prop, string other)
    {
        Prop = prop;
        Other = other + prop;
    }

    public string Prop { get; }
    public string Other { get; }
}
");
    }

    [TestMethod]
    public async Task ComplexExpression2_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program
{
    public Program(string prop, string other)
    {
        Prop = prop;
        Other[0] = other;
    }

    public string Prop { get; }
    private string[] Other = new string[1];
}
");
    }

    [TestMethod]
    public async Task ComplexExpression3_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program
{
    public Program(string prop, string other)
    {
        Prop = prop;
        Method();
        Other = other + prop;
    }

    private void Method()
    {
    }

    public string Prop { get; }
    public string Other { get; }
}
");
    }

    [TestMethod]
    public async Task ComplexExpression4_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program
{
    public Program(string prop, string other)
    {
        Prop = prop;

        for (int i = 0; i < 1; i++)
            Method();

        Other = other + prop;
    }

    private void Method()
    {
    }

    public string Prop { get; }
    public string Other { get; }
}
");
    }

    [TestMethod]
    public async Task SimpleClassWithExpressionBodyConstructor_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
class [|Program|]
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public Program(string prop, int other) => Prop = prop;

    public string Prop { get; }
}
", @"
class Program(string prop)
{
    public Program(string prop, int other) : this(prop) { }

    public string Prop { get; } = prop;
}
");
    }

    [TestMethod]
    public async Task NoProperty_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program
{
    public Program()
    {
    }
}
");
    }

    [TestMethod]
    public async Task NoAssignment_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program
{
    public Program(int prop)
    {
    }

    public int Prop { get; set; }
}
");
    }
}
