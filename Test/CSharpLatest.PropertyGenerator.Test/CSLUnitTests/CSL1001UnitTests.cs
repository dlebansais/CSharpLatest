#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

extern alias Analyzers;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpCodeFixVerifier<Analyzers::CSharpLatest.CSL1001UseIsNull, CSL1001CodeFixProvider>;

[TestClass]
internal partial class CSL1001UnitTests
{
    [TestMethod]
    public async Task SystemType_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.Default, @"
class Program
{
    static void Main(string[] args)
    {
        string s = args.Length > 0 ? null : ""test"";

        if ([|s == null|])
            Console.WriteLine(string.Empty);
    }
}
", @"
class Program
{
    static void Main(string[] args)
    {
        string s = args.Length > 0 ? null : ""test"";

        if (s is null)
            Console.WriteLine(string.Empty);
    }
}
", Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp7).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Replaced_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if (s is null)
            Console.WriteLine(string.Empty);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task ClassNoOverload_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.Nullable, @"
class Program
{
    static void Main(string[] args)
    {
        Foo? f = args.Length > 0 ? null : new();

        if ([|f == null|])
            Console.WriteLine(string.Empty);
    }
}

class Foo
{
}
", @"
class Program
{
    static void Main(string[] args)
    {
        Foo? f = args.Length > 0 ? null : new();

        if (f is null)
            Console.WriteLine(string.Empty);
    }
}

class Foo
{
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task ClassOverloaded_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program
{
    static void Main(string[] args)
    {
        Foo? f = args.Length > 0 ? null : new();

        if (f == null)
            Console.WriteLine(string.Empty);
    }
}

class Foo
{
    public static bool operator ==(Foo? foo1, Foo? foo2)
    {
        if (object.Equals(foo2, null)) throw new Exception(""oops"");
            return object.Equals(foo1, foo2);
    }
    public static bool operator !=(Foo? foo1, Foo? foo2)
    {
        if (object.Equals(foo2, null)) throw new Exception(""oops"");
            return !object.Equals(foo1, foo2);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task StructNoOverload_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.Nullable, @"
class Program
{
    static void Main(string[] args)
    {
        Foo? f = args.Length > 0 ? null : new();

        if ([|f == null|])
            Console.WriteLine(string.Empty);
    }
}

struct Foo
{
}
", @"
class Program
{
    static void Main(string[] args)
    {
        Foo? f = args.Length > 0 ? null : new();

        if (f is null)
            Console.WriteLine(string.Empty);
    }
}

struct Foo
{
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task StructOverloaded_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(Prologs.Nullable, @"
class Program
{
    static void Main(string[] args)
    {
        Foo? f = args.Length > 0 ? null : new();

        if (f == null)
            Console.WriteLine(string.Empty);
    }
}

struct Foo
{
    public static bool operator ==(Foo foo1, Foo foo2)
    {
        if (object.Equals(foo2, null)) throw new Exception(""oops"");
            return object.Equals(foo1, foo2);
    }
    public static bool operator !=(Foo foo1, Foo foo2)
    {
        if (object.Equals(foo2, null)) throw new Exception(""oops"");
            return !object.Equals(foo1, foo2);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Array_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.Nullable, @"
class Program
{
    static void Main(string[] args)
    {
        string[]? s = args.Length > 0 ? null : new string[] { ""test"" };

        if ([|s == null|])
            Console.WriteLine(string.Empty);
    }
}
", @"
class Program
{
    static void Main(string[] args)
    {
        string[]? s = args.Length > 0 ? null : new string[] { ""test"" };

        if (s is null)
            Console.WriteLine(string.Empty);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Decoration1_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.Nullable, @"
class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if (/*XYZ*/[|s == null|])
            Console.WriteLine(string.Empty);
    }
}
", @"
class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if (/*XYZ*/s is null)
            Console.WriteLine(string.Empty);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Decoration2_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.Nullable, @"
class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if ([|s/*XYZ*/ == null|])
            Console.WriteLine(string.Empty);
    }
}
", @"
class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if (s/*XYZ*/ is null)
            Console.WriteLine(string.Empty);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Decoration3_NoDiagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.Nullable, @"
class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if ([|s ==/*XYZ*/ null|])
            Console.WriteLine(string.Empty);
    }
}
", @"
class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if (s is null)
            Console.WriteLine(string.Empty);
    }
}
").ConfigureAwait(false);
    }

    [TestMethod]
    public async Task Decoration4_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(Prologs.Nullable, @"
class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if ([|s == null|]/*XYZ*/)
            Console.WriteLine(string.Empty);
    }
}
", @"
class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if (s is null/*XYZ*/)
            Console.WriteLine(string.Empty);
    }
}
").ConfigureAwait(false);
    }
}
