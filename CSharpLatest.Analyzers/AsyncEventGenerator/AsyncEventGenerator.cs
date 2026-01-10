namespace CSharpLatest.AsyncEventCodeGeneration;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a code generator that handles <see cref="AsyncEventAttribute"/>.
/// </summary>
[Generator]
public partial class AsyncEventGenerator : IIncrementalGenerator
{
    /// <inheritdoc cref="IIncrementalGenerator.Initialize"/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<EventModel> pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: FullyQualifiedMetadataName,
            predicate: KeepNodeForPipeline,
            transform: TransformAsyncEventAttribute);

        context.RegisterSourceOutput(pipeline, OutputEventHandlerEvent);
    }

    private static string FullyQualifiedMetadataName => $"{typeof(AsyncEventAttribute).Namespace}.{nameof(AsyncEventAttribute)}";
}
