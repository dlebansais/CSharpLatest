namespace CSharpLatest.AsyncEventHandlerGenerator.Test;

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
    [AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
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

[AsyncEventHandler]
public async Task FooAsync()
{
    await Task.Delay(0);
}
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

    [AsyncEventHandler(UseDispatcher = true, Arg)]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
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

internal class AsyncEventHandlerAttribute : Attribute
{
    public AsyncEventHandlerAttribute() { }
}

internal partial class Program
{
    [AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
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
    [AsyncEventHandler(SomeText  = true)]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestWrongTypeArgument1()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [AsyncEventHandler(UseDispatcher = 0)]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestWrongTypeArgument2()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    const int N = 0;
    [AsyncEventHandler(UseDispatcher = N)]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
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
    [AsyncEventHandler(UseDispatcher = 0)]
    void FooAsync()
    {
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestWrongReturnTypeVoid()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [AsyncEventHandler]
    public async void FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestWrongReturnTypeName()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [AsyncEventHandler]
    public async SimpleTest FooAsync()
    {
        await Task.Delay(0);
        return this;
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestWrongSuffix()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [AsyncEventHandler]
    public async Task FooAsynchronous()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNotMethodConstructor()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [AsyncEventHandler]
    public SimpleTest()
    {
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestNotMethodProperty()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [AsyncEventHandler]
    public int Test { get; set; }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestEmptyArguments()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using CSharpLatest;

public class SimpleTest
{
    [AsyncEventHandler()]
    public async Task FooAsync()
    {
        await Task.Delay(0);
        return this;
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }
}
