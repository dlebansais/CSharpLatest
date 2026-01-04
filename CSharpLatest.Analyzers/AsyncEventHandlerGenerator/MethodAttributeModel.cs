namespace CSharpLatest.AsyncEventHandler;

/// <summary>
/// Represents the model of an async event handler attribute.
/// </summary>
/// <param name="WaitUntilCompletion">The Wait Until Completion flag.</param>
/// <param name="FlowExceptionsToTaskScheduler">The Flow Exceptions To Task Scheduler flag.</param>
/// <param name="UseDispatcher">The Use Dispatcher flag.</param>
internal record MethodAttributeModel(bool WaitUntilCompletion,
                                     bool FlowExceptionsToTaskScheduler,
                                     bool UseDispatcher);
