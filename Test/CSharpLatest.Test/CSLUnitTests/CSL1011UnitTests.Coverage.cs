#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1011ImplementParamsCollection, CSL1011CodeFixProvider>;

internal partial class CSL1011UnitTests
{
    [TestMethod]
    public async Task CoverageDirective_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952
" + Prologs.IsExternalInitNoNullable, @"
    class Program
    {
        public static int Foo<T>(params T[] items)
        {
            int result = items.Length;
            return result;
        }
    }
", LanguageVersion.CSharp13).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OldLanguageVersion_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Default, @"
    class Program
    {
        public static int Foo<T>(params T[] items)
        {
            int result = items.Length;
            return result;
        }
    }
", LanguageVersion.CSharp12).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task DotNetFramework_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        public static int Foo<T>(params T[] items)
        {
            int result = items.Length;
            return result;
        }
    }
", LanguageVersion.CSharp13, FrameworkChoice.DotNetFramework).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OldDotNetStandard_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        public static int Foo<T>(params T[] items)
        {
            int result = items.Length;
            return result;
        }
    }
", LanguageVersion.CSharp13, FrameworkChoice.OldDotNetStandard).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoFramework_NoDiagnostic()
    {
        DiagnosticDescriptor DescriptorCS0518_1 = new(
            "CS0518",
            "title",
            "Predefined type 'System.Object' is not defined or imported",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected1 = new(DescriptorCS0518_1);
        Expected1 = Expected1.WithLocation("/0/Test0.cs", 2, 11);

        DiagnosticDescriptor DescriptorCS1729 = new(
            "CS1729",
            "title",
            "'object' does not contain a constructor that takes 0 arguments",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected2 = new(DescriptorCS1729);
        Expected2 = Expected2.WithLocation("/0/Test0.cs", 2, 11);

        DiagnosticDescriptor DescriptorCS0518_2 = new(
            "CS0518",
            "title",
            "Predefined type 'System.Void' is not defined or imported",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected3 = new(DescriptorCS0518_2);
        Expected3 = Expected3.WithLocation("/0/Test0.cs", 4, 23);

        DiagnosticDescriptor DescriptorCS0656 = new(
            "CS0656",
            "title",
            "Missing compiler required member 'System.ParamArrayAttribute..ctor'",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected4 = new(DescriptorCS0656);
        Expected4 = Expected4.WithLocation("/0/Test0.cs", 4, 32);

        DiagnosticDescriptor DescriptorCS0518_3 = new(
            "CS0518",
            "title",
            "Predefined type 'System.Int32' is not defined or imported",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected5 = new(DescriptorCS0518_3);
        Expected5 = Expected5.WithLocation("/0/Test0.cs", 4, 39);

        await VerifyCS.VerifyAnalyzerAsync("", @"
    class Program
    {
        public static void Foo(params int[] items)
        {
        }
    }
", LanguageVersion.CSharp13, FrameworkChoice.None, Expected1, Expected2, Expected3, Expected4, Expected5).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task DotNetStandard_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
    class Program
    {
        public static int Foo<T>([|params T[] items|])
        {
            int result = items.Length;
            return result;
        }
    }
", @"
    class Program
    {
        public static int Foo<T>(params ReadOnlySpan<T> items)
        {
            int result = items.Length;
            return result;
        }
    }
", LanguageVersion.CSharp13, FrameworkChoice.DotNetStandard).ConfigureAwait(false);
    }
}
