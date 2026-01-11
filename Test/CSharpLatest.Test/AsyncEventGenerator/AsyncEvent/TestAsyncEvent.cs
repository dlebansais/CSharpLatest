namespace CSharpLatest.AsyncEventGenerator.Test;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using VerifyTests;

[TestFixture]
internal class TestAsyncEvent
{
    [Test]
    public async Task TestSuccess()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    [AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestSuccessWithArgs()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    [AsyncEvent]
    public partial event AsyncEventHandler<EventArgs> Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestSuccessWithSenderAndArgs()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    [AsyncEvent]
    public partial event AsyncEventHandler<string, EventArgs> Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestStatic()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    [AsyncEvent]
    public static partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestProtected()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    [AsyncEvent]
    protected partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestInternal()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    [AsyncEvent]
    internal partial event AsyncEventHandler<string, EventArgs> Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestFile()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    [AsyncEvent]
    file partial event AsyncEventHandler<string, EventArgs> Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestPrivate()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    [AsyncEvent]
    private partial event AsyncEventHandler<string, EventArgs> Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestWeirdModifierTrivia()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    [AsyncEvent]
    public
    partial event AsyncEventHandler<string, EventArgs> Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestSuccessStruct()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial struct Program
{
    [AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestSuccessRecord()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial record Program
{
    [AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestSuccessRecordStruct()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial record struct Program
{
    [AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestVirtual()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    [AsyncEvent]
    public virtual partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestOverride()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    [AsyncEvent]
    public override partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestSealed()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    [AsyncEvent]
    public sealed partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestUnsafe()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    [AsyncEvent]
    public unsafe partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestClassWithGeneric()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program<T>
{
    [AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestClassWithConstraint()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program<T>
    where T : class
{
    [AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestDuplicate()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program<T>
    where T : class
{
    [AsyncEvent]
    [AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestDoc()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

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
    [AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestDocNoTab()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
/// <summary>
/// Test doc.
/// </summary>
/// <param name=""value"">The property value.</param>
/// <returns>The getter.</returns>
    [AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestDocNoTabWithRegion()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

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
    [AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestDocMultiple()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
    /// <summary>
    /// Test doc.
    /// </summary>

    /// <param name=""value"">The property value.</param>

    /// <returns>The getter.</returns>
    [AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestUnsupportedTrivia()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{
/**/[AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestNoTrivia()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;
using CSharpLatest.Events;

internal partial class Program
{[AsyncEvent]
    public partial event AsyncEventHandler Foo;
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEvent.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }
}
