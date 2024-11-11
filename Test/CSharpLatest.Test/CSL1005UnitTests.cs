﻿namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<CSL1005SimplifyOneLineGetter, CSL1005CodeFixProvider>;

[TestClass]
public partial class CSL1005UnitTests
{
    [TestMethod]
    public async Task OneLineGetter_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get [|{ return ""Test""; }|] }
    }
", @"
    class Program
    {
        public string Prop => ""Test"";
    }
");
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
");
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
");
    }

    [TestMethod]
    public async Task Replaced_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop => ""Test"";
    }
");
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
");
    }

    [TestMethod]
    public async Task NotReturn_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get { throw new Exception(); } }
    }
");
    }

    [TestMethod]
    public async Task EmptyReturn_NoDiagnostic()
    {
        var DescriptorCS0126 = new DiagnosticDescriptor(
            "CS0126",
            "title",
            "An object of a type convertible to 'string' is required",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        var Expected = new DiagnosticResult(DescriptorCS0126);
        Expected = Expected.WithLocation("/0/Test0.cs", 15, 36);

        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get { return; } }
    }
", LanguageVersion.Default, Expected);
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
");
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
");
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
");
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
");
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
");
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
");
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
");
    }
}