#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpAnalyzerVerifier<CSL1010InitAccessorNotSupportedInFieldBackedPropertyAttribute>;

[TestClass]
public partial class CSL1010UnitTests
{
    [TestMethod]
    public async Task InitAccessor_Diagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"")]
    public int Test { get; [|init;|] }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoInitAccessor_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public int Test { get; set; }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task NoAccessor_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public int Test => 0;
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OtherAttribute_NoDiagnostic()
    {
        DiagnosticDescriptor DescriptorCS0246_1 = new(
            "CS0246",
            "title",
            "The type or namespace name 'Foo' could not be found (are you missing a using directive or an assembly reference?)",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticDescriptor DescriptorCS0246_2 = new(
            "CS0246",
            "title",
            "The type or namespace name 'FooAttribute' could not be found (are you missing a using directive or an assembly reference?)",
            "description",
            DiagnosticSeverity.Error,
            true
            );

        DiagnosticResult Expected1 = new(DescriptorCS0246_1);
        Expected1 = Expected1.WithLocation("/0/Test0.cs", 18, 6);

        DiagnosticResult Expected2 = new(DescriptorCS0246_2);
        Expected2 = Expected2.WithLocation("/0/Test0.cs", 18, 6);

        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"

internal partial class Program
{
    [Foo]
    public int Test { get; }
}
", LanguageVersion.Default, Expected1, Expected2).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task OtherAttributeSameName_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"

internal class FieldBackedPropertyAttribute : Attribute
{
}

internal partial class Program
{
    [FieldBackedProperty]
    public int Test { get; }
}
").ConfigureAwait(false);
    }
}
