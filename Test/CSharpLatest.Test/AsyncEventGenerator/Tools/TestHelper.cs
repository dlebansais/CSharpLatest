namespace CSharpLatest.AsyncEventGenerator.Test;

extern alias Analyzers;

using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using AsyncEventGenerator = Analyzers::CSharpLatest.AsyncEventCodeGeneration.AsyncEventGenerator;

internal static class TestHelper
{
    public static GeneratorDriver GetDriver(string source)
    {
        CSharpParseOptions CSharpParseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);

        List<string> PreprocessorSymbols = [];
        CSharpParseOptions = CSharpParseOptions.WithPreprocessorSymbols(PreprocessorSymbols);

        // Parse the provided string into a C# syntax tree.
        SyntaxTree SyntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions);

        // Create references for assemblies we require.
        PortableExecutableReference ReferenceBinder = MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location);
        PortableExecutableReference ReferenceContracts = MetadataReference.CreateFromFile(typeof(AsyncEventHandlerAttribute).GetTypeInfo().Assembly.Location);

        CSharpCompilationOptions Options = new(OutputKind.ConsoleApplication,
                                               reportSuppressedDiagnostics: true,
                                               platform: Platform.X64,
                                               generalDiagnosticOption: ReportDiagnostic.Error,
                                               warningLevel: 4);

        // Create a Roslyn compilation for the syntax tree.
        CSharpCompilation Compilation = CSharpCompilation.Create(
            assemblyName: "compilation",
            syntaxTrees: [SyntaxTree],
            references: [ReferenceBinder, ReferenceContracts],
            Options);

        // Create an instance of our incremental source generator.
        AsyncEventGenerator Generator = new();

        // The GeneratorDriver is used to run our generator against a compilation.
        GeneratorDriver Driver = CSharpGeneratorDriver.Create(Generator);
        Driver = Driver.WithUpdatedParseOptions(CSharpParseOptions);

        // Run the generation pass.
        Driver = Driver.RunGeneratorsAndUpdateCompilation(Compilation, out _, out _);

        return Driver;
    }
}
