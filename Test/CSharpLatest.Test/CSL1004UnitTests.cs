namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<CSL1004ConsiderUsingPrimaryConstructor, CSL1004CodeFixProvider>;

[TestClass]
public partial class CSL1004UnitTests
{
    [TestMethod]
    public async Task SimpleClassWithPropertyLast_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    [|class Program
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
    }|]
", @"
    record Program(string Prop);
");
    }
    [TestMethod]
    public async Task SimpleClassWithPropertyFirst_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    [|class Program
    {
        public string Prop { get; }

        public Program(string prop)
        {
            Prop = prop;
        }
    }|]
", @"
    record Program(string Prop);
");
    }

    [TestMethod]
    public async Task Replaced_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
class Program(string prop)
{
    public string Prop { get; } = prop;
}
");
    }

    [TestMethod]
    public async Task NoParameterCandidate_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
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
    public async Task MultipleConstructors_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
class Program
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
");
    }

    [TestMethod]
    public async Task Decoration1_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
/*XYZ*/[|class Program
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public string Prop { get; }
}|]
", @"
/*XYZ*/record Program(string Prop);
");
    }

    [TestMethod]
    public async Task Decoration2_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    [|class Program
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
    }|]/*XYZ*/
", @"
    record Program(string Prop);/*XYZ*/
");
    }

    [TestMethod]
    public async Task Decoration3_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    [|class Program/*XYZ*/
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
    }|]
", @"
    record Program(string Prop);
");
    }

    [TestMethod]
    public async Task MissingAssignment_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
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
    public async Task MultipleConstructorsExpressionBody_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
class Program
{
    public Program(string prop) => Prop = prop;

    public Program(string prop, int other)
    {
        Prop = prop;
    }

    public string Prop { get; }
}
");
    }

    [TestMethod]
    public async Task MissingAssignmentExpressionBody_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
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
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
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
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
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
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
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
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
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
    public async Task SimpleClassWithOtherProperties1_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    [|class Program
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
        public string Other { get; } = string.Empty;
    }|]
", @"
    record Program(string Prop)
{
    public string Other { get; } = string.Empty;
}
");
    }

    [TestMethod]
    public async Task SimpleClassWithOtherProperties2_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    [|class Program
    {
        public string Other { get; } = string.Empty;

        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
    }|]
", @"
    record Program(string Prop)
{
    public string Other { get; } = string.Empty;
}
");
    }

    [TestMethod]
    public async Task SimpleClassWithExpressionBodyConstructor_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
class Program
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public Program(string prop, int other) => Prop = prop;

    public string Prop { get; }
}
");
    }

    [TestMethod]
    public async Task NoProperty_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
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
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
class Program
{
    public Program(int prop)
    {
    }

    public int Prop { get; set; }
}
");
    }

    [TestMethod]
    public async Task SimpleClassWithExtraMember_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    [|class Program
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
        protected int Other;
    }|]
", @"
    record Program(string Prop)
{
    protected int Other;
}
");
    }
}
