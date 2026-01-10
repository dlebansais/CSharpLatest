namespace CSharpLatest.AsyncEvent;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents the method that will handle an event that has no event data.
/// </summary>
/// <param name="sender">The source of the event.</param>
/// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned task.</param>
public delegate Task AsyncEventHandler(object? sender, CancellationToken cancellationToken = default);
