namespace CSharpLatest.PropertyGenerator.Test;

extern alias Analyzers;

using System.Collections.Generic;
using System.Reflection;
using Analyzers::CSharpLatest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal static class TestHelper
{
    public static GeneratorDriver GetDriver(string source, bool setFieldKeywordSupport = false)
    {
        List<string> PreprocessorDirectives = [];
        if (setFieldKeywordSupport)
            PreprocessorDirectives.Add("NET9_0_OR_GREATER");

        // Parse the provided string into a C# syntax tree.
        SyntaxTree SyntaxTree = CSharpSyntaxTree.ParseText(source);
        SyntaxNode Root = SyntaxTree.GetRoot();
        CSharpParseOptions CSharpParseOptions = new(preprocessorSymbols: PreprocessorDirectives);
        SyntaxTree = SyntaxTree.WithRootAndOptions(Root, CSharpParseOptions);

        // Create references for assemblies we require.
        PortableExecutableReference ReferenceBinder = MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location);
        PortableExecutableReference ReferenceContracts = MetadataReference.CreateFromFile(typeof(FieldBackedPropertyAttribute).GetTypeInfo().Assembly.Location);

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
        PropertyGenerator Generator = new();

        // The GeneratorDriver is used to run our generator against a compilation.
        GeneratorDriver Driver = CSharpGeneratorDriver.Create(Generator);

        // Run the generation pass.
        Driver = Driver.RunGeneratorsAndUpdateCompilation(Compilation, out _, out _);

        return Driver;
    }
}
