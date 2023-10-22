namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpLatest.Test.CSharpCodeFixVerifier<
    CSharpLatest.CSL1000VariableshouldBeMadeConstant,
    CSharpLatest.CSL1000CodeFixProvider>;

public partial class CSL1000UnitTests
{
}
