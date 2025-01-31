﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1003ConsiderUsingPrimaryConstructor, CSL1003CodeFixProvider>;

[TestClass]
internal partial class CSL1003UnitTests
{
    [TestMethod]
    public async Task Replaced_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program(string prop)
{
    public string Prop { get; } = prop;
}
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
", Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp12).ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task ConstructorWithOtherAndMoreParameters_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
    class [|Program|]
    {
        public Program(string prop, int other, double more)
        {
            Prop = prop;
            Other = other;
        }

        public Program(string prop, int other)
        {
            Prop = prop;
            Other = other;
        }

        public string Prop { get; }
        public int Other { get; }
    }
", @"
class Program(string prop, int other)
{
    public Program(string prop, int other, double more) : this(prop, other)
    {
    }

    public string Prop { get; } = prop;
    public int Other { get; } = other;
}
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
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
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task ExpressionBody_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
class [|Program|]
{
    public Program(string prop) => Prop = prop;

    public Program(string prop, int other)
    {
        Prop = prop;
    }

    public string Prop { get; private set; }
}
", @"
class Program(string prop)
{
    public Program(string prop, int other) : this(prop)
    {
    }

    public string Prop { get; private set; } = prop;
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task ExpressionBodyNotAssignment_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
class Program
{
    public Program(string prop) => Initialize(this, prop);

    public Program(string prop, int other)
    {
        Prop = prop;
    }

    public string Prop { get; private set; }

    private static void Initialize(Program pThis, string prop)
    {
        pThis.Prop = prop;
    }
}
").ConfigureAwait(false);
    }
}
