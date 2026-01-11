namespace CSharpLatest.Events;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a collection of items, implemented to store them as weak references.
/// All methods are thread-safe.
/// </summary>
/// <typeparam name="TItem"></typeparam>
internal class WeakCollection<TItem>
    where TItem : class
{
    private readonly List<WeakReference<TItem>> weakReferences = [];

    /// <inheritdoc cref="List{TItem}.Count" />
    public int Count => weakReferences.Count;

    /// <summary>
    /// Attempts to add the specified item to the collection if it is not already present.
    /// </summary>
    /// <param name="handler">The handler to add to the collection. Cannot be null.</param>
    public void TryAdd(TItem handler)
    {
        WeakReference<TItem> weakReference = new(handler);

        lock (weakReferences)
        {
            foreach (WeakReference<TItem> item in weakReferences)
                if (item.TryGetTarget(out TItem? target))
                    if (target.Equals(handler))
                        return;

            weakReferences.Add(weakReference);
        }
    }

    /// <summary>
    /// Attempts to remove the specified item from the collection. Does nothing if not present.
    /// </summary>
    /// <param name="handler"></param>
    public void TryRemove(TItem handler)
    {
        lock (weakReferences)
        {
            for (int i = 0; i < weakReferences.Count; i++)
                if (weakReferences[i].TryGetTarget(out TItem? target))
                    if (target.Equals(handler))
                    {
                        weakReferences.RemoveAt(i);
                        return;
                    }
        }
    }

    /// <summary>
    /// Performs the specified action on each element of the collection.
    /// </summary>
    /// <param name="action">The action to perform on each element of the collection.</param>
    /// <returns><see langword="true"/> if the collection contains items that have been collection and deserves a cleanup; Otherwise, <see langword="false"/>.</returns>
    public bool ForEach(Action<TItem> action)
    {
        List<WeakReference<TItem>> listCopy;

        lock (weakReferences)
        {
            listCopy = [.. weakReferences];
        }

        bool isCleanupNeeded = false;

        listCopy.ForEach((weakReference) =>
        {
            if (weakReference.TryGetTarget(out TItem? item))
                action(item);
            else
                isCleanupNeeded = true;
        });

        return isCleanupNeeded;
    }

    /// <summary>
    /// Removes items that have been collected by the garbage collector.
    /// </summary>
    public void Cleanup()
    {
        List<WeakReference<TItem>> toRemove = [];

        lock (weakReferences)
        {
            foreach (WeakReference<TItem> item in weakReferences)
            {
                if (!item.TryGetTarget(out _))
                    toRemove.Add(item);
            }

            foreach (WeakReference<TItem> item in toRemove)
                _ = weakReferences.Remove(item);
        }
    }
}
