namespace CSharpLatest.PropertyGenerator.Test;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using VerifyTests;

[TestFixture]
internal class TestNoGeneration
{
    [NUnit.Framework.Test]
    public async Task TestNoNamespace()
    {
        // The source code to test
        const string Source = @"
using System;
using CSharpLatest;

public class SimpleTest
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNoClass()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

[FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
public partial int Test { get; set; }
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNoMember()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

public class SimpleTest
{
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestIndexer()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    private string[] arr = new string[100];

    [FieldBackedProperty(GetterText = ""field"")]
    public partial string this[int i]
    {
        get => arr[i];
        set => arr[i] = value;
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNoAttributeArguments()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [FieldBackedProperty]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestEmptyAttributeArguments()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [FieldBackedProperty()]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestEmptyStringAttributeArgument()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [FieldBackedProperty("""")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestInvalidAttributeArguments()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    private const int Arg = 0;

    [FieldBackedProperty(GetterText = ""field"", Arg)]
    public partial int Test { get; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestInitAccessor()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test { get; init; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestOtherAttributeSameName()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal class FieldBackedPropertyAttribute : Attribute
{
    public FieldBackedPropertyAttribute(string getterText) { GetterText = getterText; }
    public string GetterText { get; set; }
}

internal partial class Program
{
    [FieldBackedProperty(GetterText = ""field"")]
    public partial int Test { get; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestInitializerOnly()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(InitializerText = ""0"")]
    public partial int Test { get; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestInvalidAttributeArgumentName()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [FieldBackedProperty(SomeText = ""field"", SetterText = ""field = value"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNoAccessors()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = ""field = value"", InitializerText = ""0"")]
    public partial int Test => 0;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNameofGetterArgument()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [FieldBackedProperty(GetterText = nameof(field), SetterText = ""field = value"")]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNameofSetterArgument()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [FieldBackedProperty(GetterText = ""field"", SetterText = nameof(field))]
    public partial int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestEmptyArgument()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [FieldBackedProperty(GetterText = """")]
    public partial int Test { get; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestWrongTypeArgument()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [FieldBackedProperty(GetterText = 0)]
    public partial int Test { get; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNoModifiers()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [FieldBackedProperty(GetterText = """")]
    int Test { get; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }
}
