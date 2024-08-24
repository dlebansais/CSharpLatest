namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<CSL1003ConsiderUsingPrimaryConstructor, CSL1003CodeFixProvider>;

public partial class CSL1003UnitTests
{
    [TestMethod]
    public async Task CoverageDirective_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952
#nullable enable

using System;

class Program
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public string Prop { get; }
}
");
    }

    [TestMethod]
    public async Task ClassWithoutName_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class{|CS1001:|}
{
}
");
    }

    [TestMethod]
    public async Task OldLanguageVersion_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public string Prop { get; }
}
", Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp11);
    }
}
