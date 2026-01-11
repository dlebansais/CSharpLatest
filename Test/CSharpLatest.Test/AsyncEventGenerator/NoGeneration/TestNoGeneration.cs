namespace CSharpLatest.AsyncEventGenerator.Test;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using VerifyTests;

[TestFixture]
internal class TestNoGeneration
{
    [Test]
    public async Task TestNoNamespace()
    {
        // The source code to test
        const string Source = @"
using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

public class SimpleTest
{
    [AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [Test]
    public async Task TestNoClass()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

[AsyncEvent]
public partial event AsyncEventHandler Foo;
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [Test]
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

    [Test]
    public async Task TestOtherAttributeSameName()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal class AsyncEventAttribute : Attribute
{
    public AsyncEventAttribute() { }
}

internal partial class Program
{
    [AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [Test]
    public async Task TestNoModifiers()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

public class SimpleTest
{
    [AsyncEvent]
    public event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [Test]
    public async Task TestWrongType()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

public class SimpleTest
{
    [AsyncEvent]
    public partial event EventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [Test]
    public async Task TestNoEventWithAccessors()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

public class SimpleTest
{
    [AsyncEvent]
    public partial event AsyncEventHandler Foo
    {
        add { }
        remove { }
    }
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [Test]
    public async Task TestVariableDeclaration()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

public class SimpleTest
{
    [AsyncEvent]
    public AsyncEventHandler Foo;
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [Test]
    public async Task TestWrongGenericType()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

public class SimpleTest
{
    [AsyncEvent]
    public partial event EventHandler<EventArgs> Foo;
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [Test]
    public async Task TestTooManyGenericArguments()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

public class SimpleTest
{
    [AsyncEvent]
    public partial event AsyncEventHandler<string, string, string> Foo;
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }

    [Test]
    public async Task TestPredefinedType()
    {
        // The source code to test
        const string Source = @"
namespace Contracts.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

public class SimpleTest
{
    [AsyncEvent]
    public partial event void Foo;
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyNoGeneration.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(0).Items);
    }
}
