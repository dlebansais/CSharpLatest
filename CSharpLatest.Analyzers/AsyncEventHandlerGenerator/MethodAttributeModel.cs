namespace CSharpLatest.AsyncEventHandlerCodeGeneration;

/// <summary>
/// Represents the model of an async event handler attribute.
/// </summary>
/// <param name="WaitUntilCompletion">The Wait Until Completion flag.</param>
/// <param name="UseDispatcher">The Use Dispatcher flag.</param>
internal record MethodAttributeModel(bool WaitUntilCompletion,
                                     bool UseDispatcher);
