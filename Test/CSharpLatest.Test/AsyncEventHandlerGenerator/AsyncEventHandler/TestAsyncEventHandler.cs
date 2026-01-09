namespace CSharpLatest.AsyncEventHandlerGenerator.Test;

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using VerifyTests;

[TestFixture]
internal class TestAsyncEventHandler
{
    [NUnit.Framework.Test]
    public async Task TestSuccess()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using System.Threading.Tasks;
using CSharpLatest;

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
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestStatic()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler]
    public static async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestProtected()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler]
    protected async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestInternal()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler]
    internal async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestFile()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler]
    file async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestPrivate()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler]
    private async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestWeirdModifierTrivia()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler]
    public
    static async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

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
    [AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

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
    [AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

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
    [AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestVirtual()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler]
    public virtual async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestOverride()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler]
    public override async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestSealed()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler]
    public sealed async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestUnsafe()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler]
    public unsafe async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestWaitUntilCompletion()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler(WaitUntilCompletion = true)]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestWaitUntilCompletionFalse()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler(WaitUntilCompletion = false)]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestWaitUntilCompletionAndUseDispatcher()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler(WaitUntilCompletion = true, UseDispatcher = true)]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestUseDispatcher()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler(UseDispatcher = true)]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }

    [NUnit.Framework.Test]
    public async Task TestUseDispatcherFalse()
    {
        // The source code to test
        const string Source = @"
namespace CSharpLatest.TestSuite;

using System;
using CSharpLatest;

internal partial class Program
{
    [AsyncEventHandler(UseDispatcher = false)]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

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
    [AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

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
    [AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

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
    [AsyncEventHandler(UseDispatcher = true)]
    [AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

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
    [AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

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
    [AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

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
    [AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

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
    [AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

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
/**/[AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

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
{[AsyncEventHandler]
    public async Task FooAsync()
    {
        await Task.Delay(0);
    }
}
";

        // Pass the source code to the helper and snapshot test the output.
        GeneratorDriver Driver = TestHelper.GetDriver(Source);
        VerifyResult Result = await VerifyAsyncEventHandler.Verify(Driver).ConfigureAwait(false);

        Assert.That(Result.Files, Has.Exactly(1).Items);
    }
}
