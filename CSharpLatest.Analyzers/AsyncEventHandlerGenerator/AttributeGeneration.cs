namespace CSharpLatest.AsyncEventHandlerCodeGeneration;

/// <summary>
/// Describes how to handle an attribute.
/// </summary>
public enum AttributeGeneration
{
    /// <summary>
    /// The attribute is invalid.
    /// </summary>
    Invalid,

    /// <summary>
    /// The attribute is valid and should be generated.
    /// </summary>
    Valid,
}
