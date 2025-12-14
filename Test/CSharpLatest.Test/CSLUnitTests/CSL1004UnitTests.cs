#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1004ConsiderUsingRecord, CSL1004CodeFixProvider>;

[TestClass]
internal partial class CSL1004UnitTests
{
    [TestMethod]
    public async Task SimpleClassWithPropertyLast_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class [|Program|]
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
    }
", @"
    record Program(string Prop);
", LanguageVersion.CSharp9).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task SimpleClassWithPropertyFirst_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class [|Program|]
    {
        public string Prop { get; }

        public Program(string prop)
        {
            Prop = prop;
        }
    }
", @"
    record Program(string Prop);
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Replaced_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
class Program(string prop)
{
    public string Prop { get; } = prop;
}
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Decoration1_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
/*XYZ*/class [|Program|]
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public string Prop { get; }
}
", @"
/*XYZ*/record Program(string Prop);
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Decoration2_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class [|Program|]
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
    }/*XYZ*/
", @"
    record Program(string Prop);/*XYZ*/
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Decoration3_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class [|Program|]/*XYZ*/
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
    }
", @"
    record Program(string Prop);
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task SimpleClassWithOtherProperties1_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class [|Program|]
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
        public string Other { get; } = string.Empty;
    }
", @"
    record Program(string Prop)
{
    public string Other { get; } = string.Empty;
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task SimpleClassWithOtherProperties2_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class [|Program|]
    {
        public string Other { get; } = string.Empty;

        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
    }
", @"
    record Program(string Prop)
{
    public string Other { get; } = string.Empty;
}
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task SimpleClassWithExtraMember_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class [|Program|]
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
        protected int Other;
    }
", @"
    record Program(string Prop)
{
    protected int Other;
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task BaseNotRecord_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class ProgramBase
    {
    }

    class Program : ProgramBase
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task UnknownBase_NoDiagnostic()
    {
        DiagnosticDescriptor DescriptorCS0246 = new(
            "CS0246",
            "title",
            "The type or namespace name 'ProgramBase' could not be found (are you missing a using directive or an assembly reference?)",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected = new(DescriptorCS0246);
        Expected = Expected.WithLocation("/0/Test0.cs", 15, 21);

        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program : ProgramBase
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
    }
", LanguageVersion.Default, FrameworkChoice.Default, Expected).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task UnknownGenericBase_NoDiagnostic()
    {
        DiagnosticDescriptor DescriptorCS0246 = new(
            "CS0246",
            "title",
            "The type or namespace name 'List<>' could not be found (are you missing a using directive or an assembly reference?)",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected = new(DescriptorCS0246);
        Expected = Expected.WithLocation("/0/Test0.cs", 15, 21);

        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program : List<string>
    {
        public Program(string prop)
        {
            Prop = prop;
        }

        public string Prop { get; }
    }
", LanguageVersion.Default, FrameworkChoice.Default, Expected).ConfigureAwait(false);
    }
}
