namespace CSharpLatest.AsyncEvent;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents a dispatcher for asynchronous events with no arguments type.
/// </summary>
public class AsyncEventDispatcher
{
    private readonly ConcurrentDictionary<int, WeakReference<AsyncEventHandler>> _handlers = new();

    /// <summary>
    /// Registers an asynchronous event handler.
    /// </summary>
    /// <param name="handler">The event handler.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
    public void Register(AsyncEventHandler handler)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        WeakReference<AsyncEventHandler> weakReference = new(handler);
        int hashCode = handler.GetHashCode();
        _handlers[hashCode] = weakReference;
    }

    /// <summary>
    /// Unregisters an asynchronous event handler.
    /// </summary>
    /// <param name="handler">The event handler.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
    public void Unregister(AsyncEventHandler handler)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        int hashCode = handler.GetHashCode();
        _ = _handlers.TryRemove(hashCode, out _);
    }

    /// <summary>
    /// Inkokes all registered asynchronous event handlers.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned task.</param>
    public async Task InvokeAsync(object? sender, CancellationToken cancellationToken = default)
    {
        ICollection<WeakReference<AsyncEventHandler>> weakReferenceList = _handlers.Values;
        List<Task> tasks = [];
        bool isCleanupNeeded = false;

        foreach (WeakReference<AsyncEventHandler> weakReference in weakReferenceList)
            if (weakReference.TryGetTarget(out AsyncEventHandler? handler))
            {
                Task task = handler.Invoke(sender, cancellationToken);
                tasks.Add(task);
            }
            else
            {
                isCleanupNeeded = true;
            }

        if (isCleanupNeeded)
            Cleanup();

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private void Cleanup()
    {
        foreach (KeyValuePair<int, WeakReference<AsyncEventHandler>> entry in _handlers)
        {
            int key = entry.Key;
            WeakReference<AsyncEventHandler> weakReference = entry.Value;

            if (!weakReference.TryGetTarget(out _))
                _ = _handlers.TryRemove(key, out _);
        }
    }
}

