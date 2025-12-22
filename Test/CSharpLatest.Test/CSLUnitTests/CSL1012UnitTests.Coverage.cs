#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<Analyzers::CSharpLatest.CSL1012UseSystemThreadingLockToLock>;

internal partial class CSL1012UnitTests
{
    [TestMethod]
    public async Task CoverageDirective_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952
" + Prologs.Default, @"
    class Program
    {
        private readonly object _myObj = new();

        public void Foo()
        {
            lock (_myObj)
            {
            }
        }
    }
", LanguageVersion.CSharp13, FrameworkChoice.DotNet9).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OldLanguageVersion_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        private readonly object _myObj = new();

        public void Foo()
        {
            lock (_myObj)
            {
            }
        }
    }
", LanguageVersion.CSharp12, FrameworkChoice.DotNet9).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OldFramework1_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        private readonly object _myObj = new();

        public void Foo()
        {
            lock (_myObj)
            {
            }
        }
    }
", LanguageVersion.CSharp13, FrameworkChoice.DotNet8).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OldFramework2_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    namespace System.Threading
    {
        class Lock
        {
        }
    }

    class Program
    {
        private readonly object _myObj = new();

        public void Foo()
        {
            lock (_myObj)
            {
            }
        }
    }
", LanguageVersion.CSharp13, FrameworkChoice.DotNet8).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task DotNetFramework_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    namespace System.Threading
    {
        class Lock
        {
        }
    }

    class Program
    {
        private readonly object _myObj = new();

        public void Foo()
        {
            lock (_myObj)
            {
            }
        }
    }
", LanguageVersion.CSharp13, FrameworkChoice.DotNetFramework).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task DotNetStandard_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    namespace System.Threading
    {
        class Lock
        {
        }
    }

    class Program
    {
        private readonly object _myObj = new();

        public void Foo()
        {
            lock (_myObj)
            {
            }
        }
    }
", LanguageVersion.CSharp13, FrameworkChoice.DotNetStandard).ConfigureAwait(false);
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
        Expected1 = Expected1.WithLocation("/0/Test0.cs", 4, 15);

        DiagnosticDescriptor DescriptorCS1729_1 = new(
            "CS1729",
            "title",
            "'object' does not contain a constructor that takes 0 arguments",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected2 = new(DescriptorCS1729_1);
        Expected2 = Expected2.WithLocation("/0/Test0.cs", 4, 15);

        DiagnosticDescriptor DescriptorCS0518_2 = new(
            "CS0518",
            "title",
            "Predefined type 'System.Object' is not defined or imported",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected3 = new(DescriptorCS0518_2);
        Expected3 = Expected3.WithLocation("/0/Test0.cs", 9, 11);

        DiagnosticDescriptor DescriptorCS1729_2 = new(
            "CS1729",
            "title",
            "'object' does not contain a constructor that takes 0 arguments",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected4 = new(DescriptorCS1729_2);
        Expected4 = Expected4.WithLocation("/0/Test0.cs", 9, 11);

        DiagnosticDescriptor DescriptorCS0518_3 = new(
            "CS0518",
            "title",
            "Predefined type 'System.Object' is not defined or imported",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected5 = new(DescriptorCS0518_3);
        Expected5 = Expected5.WithLocation("/0/Test0.cs", 11, 26);

        DiagnosticDescriptor DescriptorCS0518_4 = new(
            "CS0518",
            "title",
            "Predefined type 'System.Void' is not defined or imported",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected6 = new(DescriptorCS0518_4);
        Expected6 = Expected6.WithLocation("/0/Test0.cs", 13, 16);

        await VerifyCS.VerifyAnalyzerAsync("", @"
    namespace System.Threading
    {
        class Lock
        {
        }
    }

    class Program
    {
        private readonly object _myObj = new();

        public void Foo()
        {
            lock (_myObj)
            {
            }
        }
    }
", LanguageVersion.CSharp13, FrameworkChoice.None, Expected1, Expected2, Expected3, Expected4, Expected5, Expected6).ConfigureAwait(false);
    }
}
