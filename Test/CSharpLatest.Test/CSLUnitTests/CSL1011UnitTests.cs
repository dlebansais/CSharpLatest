#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1011ImplementParamsCollection, CSL1011CodeFixProvider>;

[TestClass]
internal partial class CSL1011UnitTests
{
    [TestMethod]
    public async Task SimpleParams_Diagnostic()
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
", LanguageVersion.CSharp13).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task ExplicitType_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
    class Program
    {
        public static int Foo([|params int[] items|])
        {
            int result = items.Length;
            return result;
        }
    }
", @"
    class Program
    {
        public static int Foo(params ReadOnlySpan<int> items)
        {
            int result = items.Length;
            return result;
        }
    }
", LanguageVersion.CSharp13).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task ExistingOverride1_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        public static int Foo<T>(params T[] items)
        {
            int result = items.Length;
            return result;
        }

        public static int Foo<T>(params ReadOnlySpan<T> items)
        {
            int result = items.Length;
            return result;
        }
    }
", LanguageVersion.CSharp13).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task ParameterNotUsed_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        public static int Foo<T>(params T[] items)
        {
            return 0;
        }
    }
", LanguageVersion.CSharp13).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Constructor_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        public Program(params int[] items)
        {
            _items = items;
        }

        int[] _items;
    }
", LanguageVersion.CSharp13).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Indexer_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        public int this[int index, params int[] items]
        {
            get { return items[index]; }
        }
    }
", LanguageVersion.CSharp13).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NotParams_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        public static T Foo<T>(T[] items)
        {
            return items[0];
        }
    }
", LanguageVersion.CSharp13).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NotArray_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        public static T Foo<T>(params System.Collections.Generic.List<T> items)
        {
            return items[0];
        }
    }
", LanguageVersion.CSharp13).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task InvalidMethod1_NoDiagnostic()
    {
        DiagnosticDescriptor DescriptorCS0501 = new(
            "CS0501",
            "title",
            "'Program.Foo(params int[])' must declare a body because it is not marked abstract, extern, or partial",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected1 = new(DescriptorCS0501);
        Expected1 = Expected1.WithLocation("/0/Test0.cs", 9, 27);

        DiagnosticDescriptor DescriptorCS1002 = new(
            "CS1002",
            "title",
            "; expected",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected2 = new(DescriptorCS1002);
        Expected2 = Expected2.WithLocation("/0/Test0.cs", 9, 50);

        await VerifyCS.VerifyAnalyzerAsync(@"
    partial class Program
    {
        public static int Foo(params int[] items)
    }
", LanguageVersion.CSharp13, FrameworkChoice.Default, Expected1, Expected2).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task SimpleParamsExpressionBody_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
    class Program
    {
        public static int Foo<T>([|params T[] items|])
            => items.Length;
    }
", @"
    class Program
    {
        public static int Foo<T>(params ReadOnlySpan<T> items)
            => items.Length;
    }
", LanguageVersion.CSharp13).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OutParam_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        public static void Foo<T>(out T[] items)
        {
            items = new T[0];
        }
    }
", LanguageVersion.CSharp13).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task WithOverride1_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
    class Program
    {
        public static int Foo<T>([|params T[] items|])
        {
            int result = items.Length;
            return result;
        }

        public int Dummy { get; set; }

        public void NotFoo1()
        {
        }

        public static int NotFoo2(int n)
        {
            return n;
        }

        public static int NotFoo<T>(params System.Collections.Generic.List<T> items)
        {
            int result = items.Count;
            return result;
        }

        public static int NotFoo<T>(params ReadOnlySpan<T> items)
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

        public int Dummy { get; set; }

        public void NotFoo1()
        {
        }

        public static int NotFoo2(int n)
        {
            return n;
        }

        public static int NotFoo<T>(params System.Collections.Generic.List<T> items)
        {
            int result = items.Count;
            return result;
        }

        public static int NotFoo<T>(params ReadOnlySpan<T> items)
        {
            int result = items.Length;
            return result;
        }
    }
", LanguageVersion.CSharp13).ConfigureAwait(false);
    }
}
