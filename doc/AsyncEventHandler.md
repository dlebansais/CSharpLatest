# The `AsyncEventHandler` attribute

You can use this attribute to indicate that a method is an asynchronous event handler. This attribute is defined in the `CSharpLatest` namespace.

```cs
using CSharpLatest;

[AsyncEventHandler]
async Task OnWebResponseAsync()
{
}
```

It will generate an event handler like this:

```cs
// Auto-generated code
private void OnWebResponse()
{
    // Calls OnWebResponseAsync() and handles exceptions
}
```

The method must be marked `async`, its name must end with `Async` and it must return `Task` or `ValueTask`. Note that `Task<>` is not supported, the event handler is not supposed to return a value (this is typically done through a mutable argument).

The attribute supports methods with parameters. In that case the generated code will pass the parameters to the async method. Note that `ref`, `in`, `out` and other modifiers are not supported.

```cs
[AsyncEventHandler]
async Task OnWebResponseAsync(object sender, NetworkEventArgs e)
{
}
```

It will generate an event handler like this:

```cs
// Auto-generated code
private void OnWebResponse(object sender, NetworkEventArgs e)
{
    // Calls OnWebResponseAsync(sender, e) and handles exceptions
}
```

### Concurrency

Since the handler is asynchronous, it can be configured to decide whether to allow concurrent executions with other handlers or not. When using the `AsyncEventHandler` attribute, this can be set via the `WaitUntilCompletion` property. The default is `false`, meaning that the generated code returns immediately, allowing other handlers to execute. If it instead is set to `true`, subsequent handlers will execute only after the asynchronous task has completed.

### Handling asynchronous exceptions

When the handler awaits the completion of an invocation, any exceptions will naturally be thrown on the same synchronization context. That usually means that exceptions being thrown would just crash the app, which is a behavior consistent with that of synchronous handlers (where exceptions being thrown will also crash the app).

### WPF GUI thread

If the event handler needs to interact with WPF GUI elements, it must ensure that the code runs on the GUI thread. For this purpose, the generated code can use `Dispatcher.InvokeAsync` to marshal the call to the GUI thread, instead of the generic `Task.Run`. This behavior is controlled with the `UseDispatcher` property:

```cs
[AsyncEventHandler(UseDispatcher = true)]
async Task OnButtonClick(object sender, RoutedEventArgs e)
{
    // Assuming x:Name="myButton", disables the button
    // If this code doesn't run on the GUI thread, it will throw an exception
    myButton.IsEnabled = false;
}
```
