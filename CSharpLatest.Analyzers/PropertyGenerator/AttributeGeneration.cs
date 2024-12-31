namespace CSharpLatest;

/// <summary>
/// Describes how to handle a contract attribute.
/// </summary>
public enum AttributeGeneration
{
    /// <summary>
    /// The attribute is invalid.
    /// </summary>
    Invalid,

    /// <summary>
    /// The attribute is for debug only, ignore it for release.
    /// </summary>
    DebugOnly,

    /// <summary>
    /// The attribute is valid and should be generated.
    /// </summary>
    Valid,
}
