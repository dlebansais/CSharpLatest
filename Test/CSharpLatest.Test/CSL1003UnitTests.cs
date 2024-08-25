namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<CSL1003ConsiderUsingPrimaryConstructor, CSL1003CodeFixProvider>;

[TestClass]
public partial class CSL1003UnitTests
{
    [TestMethod]
    public async Task SimpleClassWithProperties_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
#nullable enable

using System;

[|class Program
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public string Prop { get; }
}|]
", @"
#nullable enable

using System;

class Program(string prop)
{
    public string Prop { get; } = prop;
}
");
    }

    [TestMethod]
    public async Task Replaced_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class Program(string prop)
{
    public string Prop { get; } = prop;
}
");
    }

    [TestMethod]
    public async Task NoParameterCandidate_NoDiagnostic()
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

    public Program(int prop)
    {
        Prop = prop.ToString();
    }

    public string Prop { get; }
}
");
    }

    [TestMethod]
    public async Task MultipleConstructors_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
#nullable enable

using System;

[|class Program
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
}|]
", @"
#nullable enable

using System;

class Program(string prop)
{
    public Program(string prop, int other) : this(prop)
    {
    }

    public string Prop { get; } = prop;
}
");
    }

#if DISABLED
    [TestMethod]
    public async Task Decoration1_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
#nullable enable

using System;

/*XYZ*/[|class Program
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public string Prop { get; }
}|]
", @"
#nullable enable

using System;

/*XYZ*/
class Program(string prop)
{
    public string Prop { get; } = prop;
}
");
    }
#endif

    [TestMethod]
    public async Task Decoration2_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
#nullable enable

using System;

[|class Program
{
    public Program(string prop)
    {
        Prop = prop;
    }

    public string Prop { get; }
}|]/*XYZ*/
", @"
#nullable enable

using System;

class Program(string prop)
{
    public string Prop { get; } = prop;
}/*XYZ*/
");
    }
}
