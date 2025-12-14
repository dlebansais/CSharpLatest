#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<Analyzers::CSharpLatest.CSL1012UseSystemThreadingLockToLock>;

[TestClass]
internal partial class CSL1012UnitTests
{
    [TestMethod]
    public async Task NotLockType_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        private readonly object _myObj = new();

        public void Foo()
        {
            lock ([|_myObj|])
            {
            }
        }
    }
", LanguageVersion.CSharp13, FrameworkChoice.DotNet9).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task LockType_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        private readonly System.Threading.Lock _myObj = new();

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
    public async Task GlobalStatement_NoDiagnostic()
    {
        DiagnosticDescriptor DescriptorCS0116 = new(
            "CS0116",
            "title",
            "A namespace cannot directly contain members such as fields, methods or statements",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected1 = new(DescriptorCS0116);
        Expected1 = Expected1.WithLocation("/0/Test0.cs", 7, 44);

        DiagnosticDescriptor DescriptorCS8805 = new(
            "CS8805",
            "title",
            "Program using top-level statements must be an executable.",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected2 = new(DescriptorCS8805);
        Expected2 = Expected2.WithLocation("/0/Test0.cs", 9, 5);

        DiagnosticDescriptor DescriptorCS0103 = new(
            "CS0103",
            "title",
            "The name '_myObj' does not exist in the current context",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected3 = new(DescriptorCS0103);
        Expected3 = Expected3.WithLocation("/0/Test0.cs", 9, 11);

        await VerifyCS.VerifyAnalyzerAsync(@"
    private readonly System.Threading.Lock _myObj = new();

    lock (_myObj)
    {
    }
", LanguageVersion.CSharp13, FrameworkChoice.DotNet9, Expected1, Expected2, Expected3).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task CustomType_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"

    class MyLock
    {
    }

    class Program
    {
        private readonly MyLock _myObj = new();

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
    public async Task InvalidLock_NoDiagnostic()
    {
        DiagnosticDescriptor DescriptorCS1525 = new(
            "CS1525",
            "title",
            "Invalid expression term ')'",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected = new(DescriptorCS1525);
        Expected = Expected.WithLocation("/0/Test0.cs", 13, 19);

        await VerifyCS.VerifyAnalyzerAsync(@"
    class Program
    {
        private readonly object _myObj = new();

        public void Foo()
        {
            lock ()
            {
            }
        }
    }
", LanguageVersion.CSharp13, FrameworkChoice.DotNet9, Expected).ConfigureAwait(false);
    }
}
