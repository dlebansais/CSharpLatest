namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<CSL1006SimplifyOneLineSetter, CSL1006CodeFixProvider>;

[TestClass]
public partial class CSL1006UnitTests
{
    [TestMethod]
    public async Task OneLineSetter_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { set [|{ _prop = value; }|] }
        private string _prop = string.Empty;
    }
", @"
    class Program
    {
        public string Prop { set => _prop = value; }
        private string _prop = string.Empty;
    }
");
    }

    [TestMethod]
    public async Task OneLineSetterWithGetter_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { get => _prop; set [|{ _prop = value; }|] }
        private string _prop = string.Empty;
    }
", @"
    class Program
    {
        public string Prop { get => _prop; set => _prop = value; }
        private string _prop = string.Empty;
    }
");
    }

    [TestMethod]
    public async Task Replaced_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { set => _prop = value; }
        private string _prop = string.Empty;
    }
");
    }

    [TestMethod]
    public async Task MultipleStatements_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop
        {
            set
            {
                _prop = value;
                _prop = _prop + _prop;
            }
        }

        private string _prop = string.Empty;
    }
");
    }

    [TestMethod]
    public async Task NotExpression_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop
        {
            set
            {
                if (value != string.Empty)
                    _prop = value;
            }
        }

        private string _prop = string.Empty;
    }
");
    }

    [TestMethod]
    public async Task MultiLine_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop
        {
            set
            {
                _prop =
                        value;
            }
        }

        private string _prop = string.Empty;
    }
");
    }

    [TestMethod]
    public async Task OneLineGetterWithLeadingTrivia_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { set/**/[|{ _prop = value; }|] }
        private string _prop = string.Empty;
    }
", @"
    class Program
    {
        public string Prop { set/**/=> _prop = value; }
        private string _prop = string.Empty;
    }
");
    }

    [TestMethod]
    public async Task OneLineGetterWithTrailingTrivia_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.IsExternalInit, @"
    class Program
    {
        public string Prop { set [|{ _prop = value; }|]/**/}
        private string _prop = string.Empty;
    }
", @"
    class Program
    {
        public string Prop { set => _prop = value;/**/}
        private string _prop = string.Empty;
    }
");
    }
}
