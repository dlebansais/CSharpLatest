namespace CSharpLatest.AsyncEventHandlerCodeGeneration;

/// <summary>
/// Represents the model of an async event handler.
/// </summary>
/// <param name="Namespace">The namespace containing the class that contains the method.</param>
/// <param name="UsingsBeforeNamespace">Using directives before the namespace declaration.</param>
/// <param name="UsingsAfterNamespace">Using directives after the namespace declaration.</param>
/// <param name="ClassName">The name of the class containing the method.</param>
/// <param name="DeclarationTokens">The token(s) to use for declaration (either 'class', 'struct', 'record' or 'record struct').</param>
/// <param name="FullClassName">The name of the class with type parameter and constraints.</param>
/// <param name="SymbolName">The mathod name.</param>
/// <param name="MethodAttributeModel">The method attribute model.</param>
/// <param name="Documentation">The method documentation, if any.</param>
/// <param name="GeneratedMethodDeclaration">The generated method.</param>
internal record MethodModel(string Namespace,
                            string UsingsBeforeNamespace,
                            string UsingsAfterNamespace,
                            string ClassName,
                            string DeclarationTokens,
                            string FullClassName,
                            string SymbolName,
                            MethodAttributeModel MethodAttributeModel,
                            string Documentation,
                            string GeneratedMethodDeclaration);
