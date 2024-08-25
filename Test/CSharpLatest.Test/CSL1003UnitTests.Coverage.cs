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

    [TestMethod]
    public async Task ConstructorWithBase_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class BaseProgram
{
    public BaseProgram()
    {
    }
}

class Program : BaseProgram
{
    public Program(string prop) : base()
    {
        Prop = prop;
    }

    public string Prop { get; }
}
");
    }

    [TestMethod]
    public async Task ConstructorWithOtherParameters_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class Program
{
    public Program(string prop, int other)
    {
        Prop = prop;
    }

    public Program(string prop, double other)
    {
        Prop = prop;
    }

    public string Prop { get; }
}
");
    }

    [TestMethod]
    public async Task ConstructorWithOtherAssignment_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class Program
{
    public Program(string prop)
    {
        Prop = prop;
    }

    private string Prop;
}
");
    }

    [TestMethod]
    public async Task PropertyWithInitializer_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class Program
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public string Prop { get; } = string.Empty;
}
");
    }

    [TestMethod]
    public async Task ConstructorWithOtherInstructions_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class Program
{
    public Program(string prop)
    {
        Prop = prop;
        Other();
    }

    private void Other()
    {
    }

    public string Prop { get; }
}
");
    }

    [TestMethod]
    public async Task OtherConstructorWithoutAssignments_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class Program
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public Program(string prop, int other)
    {
    }

    public string Prop { get; }
}
");
    }
}
