namespace CSharpLatest.AsyncEventCodeGeneration;

/// <summary>
/// Values for a dispatcher kind.
/// </summary>
internal enum DispatcherKind
{
    /// <summary>
    /// No arguments nor sender.
    /// </summary>
    Simple,

    /// <summary>
    /// Arguments only.
    /// </summary>
    WithEventArgs,

    /// <summary>
    /// With sender and arguments.
    /// </summary>
    WithSenderAndEventArgs,
}
