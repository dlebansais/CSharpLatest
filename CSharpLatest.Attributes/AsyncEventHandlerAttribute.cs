namespace CSharpLatest;

using System;

/// <summary>
/// Represents an attribute to automatically generate an event handler that can run the async method that follows.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class AsyncEventHandlerAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether to wait for completion of the async method before continuing.
    /// </summary>
    public bool WaitUntilCompletion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to flow exceptions to the TaskScheduler.
    /// </summary>
    public bool FlowExceptionsToTaskScheduler { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use the Dispatcher to invoke the async method on the GUI thread.
    /// </summary>
    public bool UseDispatcher { get; set; }
}
