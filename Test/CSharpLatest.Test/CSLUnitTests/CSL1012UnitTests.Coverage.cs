#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
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
}
