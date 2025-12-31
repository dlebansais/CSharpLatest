using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: DiscoverInternals]
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace CSharpLatest.PropertyGenerator.Test;

using System.Runtime.CompilerServices;
using VerifyTests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        try
        {
            VerifySourceGenerators.Initialize();
        }
        catch
        {
            // Ignore
        }
    }
}
