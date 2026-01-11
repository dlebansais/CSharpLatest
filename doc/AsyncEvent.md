# The `AsyncEvent` attribute

You can use this attribute to declare an event that runs handlers using `async`/`await`. This attribute and the `AsyncEventHandler` delegate are defined in the `CSharpLatest` namespace.

Events declared with this attribute have the additional benefits that unregistration is not required to avoid memory leaks, and that exceptions thrown inside handlers are propagated back to the caller.

## Declaring an asynchronous event

```cs
using CSharpLatest;

// Declares an event without parameters.
[AsyncEvent]
public partial event AsyncEventHandler MyEvent;

// Declares an event with custom parameters of type MyEventArgs.
// MyEventArgs must inherit from System.EventArgs. 
[AsyncEvent]
public partial event AsyncEventHandler<MyEventArgs> MyEvent;

// Declares an event with a custom sender of type MySender and custom parameters of type MyEventArgs.
// MySender must be a class.
// MyEventArgs must inherit from System.EventArgs. 
[AsyncEvent]
public partial event AsyncEventHandler<MySender, MyEventArgs> MyEvent;
```

Each of these declarations will generate an event handler like this:

```cs
// Auto-generated code
public partial event AsyncEventHandler MyEvent
{
    add => __myEvent.Register(value);
    remove => __myEvent.Unregister(value);
}

private readonly AsyncEventDispatcher __myEvent = new();

private async Task RaiseMyEvent(CancellationToken cancellationToken = default)
    => await __myEvent.InvokeAsync(this, cancellationToken).ConfigureAwait(false);

// Auto-generated code with custom parameters
public partial event AsyncEventHandler<MyEventArgs> MyEvent
{
    add => __myEvent.Register(value);
    remove => __myEvent.Unregister(value);
}

private readonly AsyncEventDispatcher<MyEventArgs> __myEvent = new();

private async Task RaiseMyEvent(MyEventArgs args, CancellationToken cancellationToken = default)
    => await __myEvent.InvokeAsync(this, args, cancellationToken).ConfigureAwait(false);

// Auto-generated code with custom sender and parameters
public partial event AsyncEventHandler<MySender, MyEventArgs> MyEvent
{
    add => __myEvent.Register(value);
    remove => __myEvent.Unregister(value);
}

private readonly AsyncEventDispatcher<MySender, MyEventArgs> __myEvent = new();

private async Task RaiseMyEvent(MySender sender, MyEventArgs args, CancellationToken cancellationToken = default)
    => await __myEvent.InvokeAsync(sender, args, cancellationToken).ConfigureAwait(false);
```

The benefit of using this attribute and `AsyncEventHandler` is that the event handlers that register can be declared as asynchronous methods and the chain of calls from invocation of the event to handling can be asynchronous uninterruptedly.

Do not confuse `AsyncEventHandler`, the delegate, with `[AsyncEventHandler]` the attribute that is used to decorate asynchronous event handler methods handling synchronous events (see [The `AsyncEventHandler` attribute](AsyncEventHandler.md)).

## Registering handlers

You can register asynchronous event handlers like this:

```cs
// Registers an event handler without parameters.
MyEvent += async (sender, cancellationToken) =>
{
    // sender is of type object
    await Task.Delay(1000, cancellationToken);
    Console.WriteLine("Event handled!");
};

// Registers an event handler with parameters.
MyEvent += async (sender, args, cancellationToken) =>
{
    // sender is of type object
    // args is of type MyEventArgs
    await Task.Delay(1000, cancellationToken);
    Console.WriteLine("Event handled!");
};

// Registers an event handler with a custom sender and custom parameters.
MyEvent += async (sender, args, cancellationToken) =>
{
    // sender is of type MySender
    // args is of type MyEventArgs
    await Task.Delay(1000, cancellationToken);
    Console.WriteLine("Event handled!");
};
```

## Unregistering handlers

Event handlers can be unregistered in the same way as regular events. Moreover, handlers are stored as weak references. If an instance not implementing `IDisposable` registers a handler, and is later removed from memory, the event will be unregistered automatically at the first opportunity.

Because of this, implementing `IDisposable` for the sole purpose of unregistering event handlers is not necessary, and will not lead to memory leaks.

## Concurrency

Inside a handler, the first `await` may cause the handler to yield, and subsequent handlers may execute immediately after that, regardless of whether the previous handler has completed or not. This means that handlers can run concurrently.

Moreover, even though this is an implementation detail, handlers are executed using `Task.WhenAll()`, meaning no guarantees are made about the order of completion of handlers.

## Handling asynchronous exceptions

Exceptions thrown inside asynchronous event handlers will propagate back to the caller of the event invocation method (e.g., `RaiseMyEvent`). If multiple handlers throw exceptions, they will be aggregated into an `AggregateException`.

## Access modifiers

The generated code uses the same access modifier as the event declaration for the event itself (i.e., `public`, `private`, etc.). The generated invocation method (e.g., `RaiseMyEvent`) and support field (e.g., `__myEvent`) are always declared as `private`.

If you want to change the access modifier of the invocation method, you can do so by declaring a separate method and desired modifiers and call the invocation method.

```cs
// Make RaiseMyEvent public
public async Task RaiseMyEventPublicly(MyEventArgs args, CancellationToken cancellationToken = default)
    => await RaiseMyEvent(args, cancellationToken).ConfigureAwait(false);
```
