[assembly: Microsoft.VisualStudio.TestTools.UnitTesting.DiscoverInternals]

namespace CSharpLatest.PropertyGenerator.Test;

using System.Runtime.CompilerServices;
using VerifyTests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
