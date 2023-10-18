namespace CSharpLatest.Test;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = CSharpLatest.Test.CSharpCodeFixVerifier<
    CSharpLatest.CSL1001Analyzer,
    CSharpLatest.CSL1001CodeFixProvider>;

[TestClass]
public class CSL1001UnitTests
{
    [TestMethod]
    public async Task SystemType_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
#nullable enable

using System;

class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if ([|s == null|])
            Console.WriteLine(string.Empty);
    }
}
", @"
#nullable enable

using System;

class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if (s is null)
            Console.WriteLine(string.Empty);
    }
}
");
    }

    [TestMethod]
    public async Task Replaced_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

class Program
{
    static void Main(string[] args)
    {
        string? s = args.Length > 0 ? null : ""test"";

        if (s is null)
            Console.WriteLine(string.Empty);
    }
}
");
    }

    [TestMethod]
    public async Task ClassNoOverload_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
#nullable enable

using System;

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
#nullable enable

using System;

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
");
    }

    [TestMethod]
    public async Task ClassOverloaded_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

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
");
    }

    [TestMethod]
    public async Task StructNoOverload_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
#nullable enable

using System;

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
#nullable enable

using System;

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
");
    }

    [TestMethod]
    public async Task StructOverloaded_NoDiagnostic()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
#nullable enable

using System;

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
");
    }

    [TestMethod]
    public async Task Array_Diagnostic()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
#nullable enable

using System;

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
#nullable enable

using System;

class Program
{
    static void Main(string[] args)
    {
        string[]? s = args.Length > 0 ? null : new string[] { ""test"" };

        if (s is null)
            Console.WriteLine(string.Empty);
    }
}
");
    }
}
