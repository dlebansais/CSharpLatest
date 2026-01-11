namespace CSharpLatest.AsyncEventCodeGeneration;

/// <summary>
/// Represents the model of an async event handler.
/// </summary>
/// <param name="Namespace">The namespace containing the class that contains the event.</param>
/// <param name="UsingsBeforeNamespace">Using directives before the namespace declaration.</param>
/// <param name="UsingsAfterNamespace">Using directives after the namespace declaration.</param>
/// <param name="ClassName">The name of the class containing the event.</param>
/// <param name="DeclarationTokens">The token(s) to use for declaration (either 'class', 'struct', 'record' or 'record struct').</param>
/// <param name="FullClassName">The name of the class with type parameter and constraints.</param>
/// <param name="SymbolName">The mathod name.</param>
/// <param name="Documentation">The event documentation, if any.</param>
/// <param name="DispatcherKind">The dispatcher kind.</param>
/// <param name="SenderType">The sender type.</param>
/// <param name="ArgumentType">The argument type.</param>
/// <param name="GeneratedEventDeclaration">The generated event.</param>
internal record EventModel(string Namespace,
                           string UsingsBeforeNamespace,
                           string UsingsAfterNamespace,
                           string ClassName,
                           string DeclarationTokens,
                           string FullClassName,
                           string SymbolName,
                           string Documentation,
                           DispatcherKind DispatcherKind,
                           string SenderType,
                           string ArgumentType,
                           string GeneratedEventDeclaration);
