namespace CSharpLatest.FieldBackedProperty;

using System.Linq;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a code generator that handles <see cref="FieldBackedPropertyAttribute"/>.
/// </summary>
[Generator]
public partial class FieldBackedPropertyGenerator : IIncrementalGenerator
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
            transform: TransformFieldBackedPropertyAttribute);

        context.RegisterSourceOutput(settings.Combine(pipeline.Collect()), OutputFieldBackedMethod);
    }

    private static string FullyQualifiedMetadataName => $"{typeof(FieldBackedPropertyAttribute).Namespace}.{nameof(FieldBackedPropertyAttribute)}";
}
