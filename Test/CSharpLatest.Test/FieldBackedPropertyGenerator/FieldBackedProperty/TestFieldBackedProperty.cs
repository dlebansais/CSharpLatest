namespace CSharpLatest.FieldBackedPropertyGenerator.Test;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using VerifyTests;

[TestFixture]
internal class TestFieldBackedProperty
{
    [NUnit.Framework.Test]
    public async Task TestSuccessNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestSuccessNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestStaticNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial static int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestStaticNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial static int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestProtectedNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    protected partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestProtectedNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    protected partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestInternalNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    internal partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestInternalNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    internal partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestFileNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    file partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestFileNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    file partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestPrivateNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    private partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestPrivateNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    private partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestWeirdModifierTriviaNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    partial
    static int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestWeirdModifierTriviaNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    partial
    static int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestSuccessStruct()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial struct Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestSuccessRecord()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial record Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestSuccessRecordStruct()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial record struct Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestVirtualNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public virtual partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestVirtualNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public virtual partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestOverrideNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public override partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestOverrideNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public override partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestSealedNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public sealed partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestSealedNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public sealed partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestUnsafeNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial unsafe int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestUnsafeNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial unsafe int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestRequiredNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public required partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestRequiredNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public required partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNoGetterNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNoGetterNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNoSetterNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", InitializerText = ""0"")]
    public partial int Test { get; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNoSetterNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", InitializerText = ""0"")]
    public partial int Test { get; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestBlockGetterNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""{ return field; }"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestBlockGetterNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""{ return field; }"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestBlockSetterNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""{ field = value; }"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestBlockSetterNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""{ field = value; }"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestCorruptedBlock()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(SetterText = ""field = value;"", InitializerText = ""0"")]
    public partial int Test { set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestClassWithGeneric()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program<T>
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestClassWithConstraint()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program<T>
    where T : class
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestDuplicateWithNoArgumentProperty()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    [FieldBackedProperty]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestDoc()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    #endregion

    #if DEBUG
    #endif

    #region Test
    /// <summary>
    /// Test doc.
    /// </summary>
    /// <param name=""value"">The property value.</param>
    /// <returns>The getter.</returns>
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestDocNoTab()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
/// <summary>
/// Test doc.
/// </summary>
/// <param name=""value"">The property value.</param>
/// <returns>The getter.</returns>
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestDocNoTabWithRegion()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    #endregion

#if DEBUG
#endif

#region Test
/// <summary>
/// Test doc.
/// </summary>
/// <param name=""value"">The property value.</param>
/// <returns>The getter.</returns>
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestDocMultiple()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    /// <summary>
    /// Test doc.
    /// </summary>

    /// <param name=""value"">The property value.</param>

    /// <returns>The getter.</returns>
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestUnsupportedTrivia()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
/**/[FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNoTrivia()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{[FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestSuccessNoInitializerNetFramework()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestSuccessNoInitializerNet9()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source, setFieldKeywordSupport: true);
        VerifyResult Result = await VerifyFieldBackedProperty.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }
}
