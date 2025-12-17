#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

#if ENABLE_CSL1013

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1013ChangeExtensionFunctionToExtensionMember, CSL1013CodeFixProvider>;

[TestClass]
internal partial class CSL1013UnitTests
{
    [TestMethod]
    public async Task SimpleExtensionMethod_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
static class Program
{
    [|public static int GetLength(this string s)
    {
        ArgumentNullException.ThrowIfNull(s);

        return s.Length;
    }|]
}
", @"
static class Program
{
    extension(string s)
    {
        public int GetLength()
        {
            ArgumentNullException.ThrowIfNull(s);

            return s.Length;
        }
    }
}
", LanguageVersion.CSharp14, FrameworkChoice.DotNet10).ConfigureAwait(false);
    }
}

#endif
