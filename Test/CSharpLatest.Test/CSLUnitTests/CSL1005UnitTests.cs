#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1005SimplifyOneLineGetter, CSL1005CodeFixProvider>;

[TestClass]
internal partial class CSL1005UnitTests
{
    [TestMethod]
    public async Task OneLineGetter_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInitNoNullable, @"
    class Program
    {
        public string Prop { get [|{ return ""Test""; }|] }
    }
", @"
    class Program
    {
        public string Prop => ""Test"";
    }
", LanguageVersion.CSharp7).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OneLineExpressionBodyGetter_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get [|=> ""Test""|]; }
    }
", @"
    class Program
    {
        public string Prop => ""Test"";
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OneLineGetterWithSetter_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get [|{ return ""Test""; }|] set { } }
    }
", @"
    class Program
    {
        public string Prop { get => ""Test""; set { } }
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Replaced_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop => ""Test"";
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task MultipleStatements_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop 
        {
            get
            {
                string Result = ""Test"";
                return Result;
            }
        }
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task ReturnFollowedByStatement_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop 
        {
            get
            {
                return ""x"";
                string Result = ""Test"";
            }
        }
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NotReturn_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get { throw new Exception(); } }
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task EmptyReturn_NoDiagnostic()
    {
        DiagnosticDescriptor DescriptorCS0126 = new(
            "CS0126",
            "title",
            "An object of a type convertible to 'string' is required",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected = new(DescriptorCS0126);
        Expected = Expected.WithLocation("/0/Test0.cs", 17, 36);

        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get { return; } }
    }
", LanguageVersion.Default, FrameworkChoice.Default, Expected).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task MultiLine_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get { return
""Test""; } }
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OneLineGetterWithLeadingTrivia_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop/**/{ get [|{ return ""Test""; }|] }
    }
", @"
    class Program
    {
        public string Prop/**/=> ""Test"";
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OneLineGetterWithTrailingTrivia_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get [|{ return ""Test""; }|] }/**/
    }
", @"
    class Program
    {
        public string Prop => ""Test"";/**/
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OneLineExpressionBodyGetterWithLeadingTrivia_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop/**/{ get [|=> ""Test""|]; }
    }
", @"
    class Program
    {
        public string Prop/**/=> ""Test"";
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OneLineExpressionBodyGetterWithTrailingTrivia_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get [|=> ""Test""|]; }/**/
    }
", @"
    class Program
    {
        public string Prop => ""Test"";/**/
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OneLineGetterWithSetterWithLeadingTrivia_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get/**/[|{ return ""Test""; }|] set { } }
    }
", @"
    class Program
    {
        public string Prop { get/**/=> ""Test""; set { } }
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OneLineGetterWithSetterWithTrailingTrivia_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get [|{ return ""Test""; }|]/**/set { } }
    }
", @"
    class Program
    {
        public string Prop { get => ""Test"";/**/set { } }
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task EventGetter_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    public delegate void RoutedEventHandler(object sender);

    class Program
    {
        public event RoutedEventHandler Foo
        {
            add { }
            remove { }
        }
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task IndexerGetter_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public int this[int index]
        {
            get { return index; }
            set { }
        }
    }
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoAccessors_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get; set; }
    }
").ConfigureAwait(false);
    }
}
