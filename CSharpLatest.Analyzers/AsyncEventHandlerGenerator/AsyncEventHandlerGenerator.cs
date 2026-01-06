namespace CSharpLatest.AsyncEventHandler;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a code generator that handles <see cref="AsyncEventHandlerAttribute"/>.
/// </summary>
[Generator]
public partial class AsyncEventHandlerGenerator : IIncrementalGenerator
{
    /// <inheritdoc cref="IIncrementalGenerator.Initialize"/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<PropertyModel> pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: FullyQualifiedMetadataName,
            predicate: KeepNodeForPipeline,
            transform: TransformAsyncEventHandlerAttribute);

        context.RegisterSourceOutput(pipeline, OutputEventHandlerMethod);
    }

    private static string FullyQualifiedMetadataName => $"{typeof(AsyncEventHandlerAttribute).Namespace}.{nameof(AsyncEventHandlerAttribute)}";
}
