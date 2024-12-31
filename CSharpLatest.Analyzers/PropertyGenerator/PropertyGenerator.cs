namespace CSharpLatest;

using System.Linq;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a code generator that handles <see cref="PropertyAttribute"/>.
/// </summary>
[Generator]
public partial class PropertyGenerator : IIncrementalGenerator
{
    /// <inheritdoc cref="IIncrementalGenerator.Initialize"/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<GeneratorSettings> Settings = context.AnalyzerConfigOptionsProvider.SelectMany(ReadSettings);

        InitializePipeline(context, Settings);
    }

    private static void InitializePipeline(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<GeneratorSettings> settings)
    {
        IncrementalValuesProvider<PropertyModel> pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: FullyQualifiedMetadataName,
            predicate: KeepNodeForPipeline,
            transform: TransformContractAttributes);

        context.RegisterSourceOutput(settings.Combine(pipeline.Collect()), OutputContractMethod);
    }

    private static string FullyQualifiedMetadataName => $"{typeof(PropertyAttribute).Namespace}.{nameof(PropertyAttribute)}";
}
