namespace CSharpLatest;

/// <summary>
/// Represents the model of a method or property contract.
/// </summary>
/// <param name="Namespace">The namespace containing the class that contains the method or property.</param>
/// <param name="ClassName">The name of the class containing the method or property.</param>
/// <param name="DeclarationTokens">The token(s) to use for declaration (either 'class', 'struct', 'record' or 'record struct').</param>
/// <param name="FullClassName">The name of the class with type parameter and constraints.</param>
/// <param name="SymbolName">The property name.</param>
/// <param name="GetterText">The getter text.</param>
/// <param name="SetterText">The setter text.</param>
/// <param name="InitializerText">The initializer text.</param>
/// <param name="Documentation">The method or property documentation, if any.</param>
/// <param name="GeneratedPropertyDeclaration">The generated property.</param>
/// <param name="GeneratedFieldDeclaration">The generated field.</param>
internal record PropertyModel(string Namespace,
                              string ClassName,
                              string DeclarationTokens,
                              string FullClassName,
                              string SymbolName,
                              string GetterText,
                              string SetterText,
                              string InitializerText,
                              string Documentation,
                              string GeneratedPropertyDeclaration,
                              string GeneratedFieldDeclaration);
