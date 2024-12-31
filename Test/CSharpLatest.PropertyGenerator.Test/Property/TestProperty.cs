namespace CSharpLatest.PropertyGenerator.Test;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using VerifyTests;

[TestFixture]
internal class TestProperty
{
    [Test]
    public async Task TestSuccessNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [Property(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyEnsure.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestSuccessNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [Property(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyEnsure.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestStaticNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [Property(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial static int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyEnsure.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestStaticNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [Property(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial static int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyEnsure.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestProtectedNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [Property(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    protected partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyEnsure.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestProtectedNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [Property(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    protected partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyEnsure.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }
}
