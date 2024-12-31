#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<CSL1009PropertyAttributeIsMissingArgument>;

[TestClass]
public partial class CSL1009UnitTests
{
    [TestMethod]
    public async Task NoArgumentProperty_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
namespace TestSuite;

internal partial class Program
{
    [[|CSharpLatest.Property|]]
    public int Test { get; }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OneArgument_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
internal partial class Program
{
    [Property(GetterText = ""field"")]
    public int Test { get; }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task EmptyArgument_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
internal partial class Program
{
    [[|Property()|]]
    public int Test { get; }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task UnsupportedAttribute_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
internal partial class Program
{
    [Obsolete]
    public int Test { get; }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoArgumentOtherAccess_NoDiagnostic()
    {
        DiagnosticDescriptor DescriptorCS7036 = new(
            "CS7036",
            "title",
            "There is no argument given that corresponds to the required parameter 'value' of 'AccessAttribute.AccessAttribute(string)'",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected = new(DescriptorCS7036);
        Expected = Expected.WithLocation("/0/Test0.cs", 13, 6);

        await VerifyCS.VerifyAnalyzerAsync(@"
namespace Test;

internal class PropertyAttribute : Attribute
{
    public PropertyAttribute(string getterText) { GetterText = getterText; }
    public string GetterText { get; set; }
}

internal partial class Program
{
    [PropertyAttribute]
    public int Test { get; }
}
", LanguageVersion.Default, Expected).ConfigureAwait(false);
    }
}
