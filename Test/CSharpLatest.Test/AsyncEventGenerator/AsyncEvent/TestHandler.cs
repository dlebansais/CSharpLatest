namespace CSharpLatest.AsyncEventGenerator.Test;

using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpLatest.Events;
using NUnit.Framework;

[TestFixture]
internal class TestHandler
{
    [Test]
    public void TestDispatcher()
    {
        AsyncEventDispatcher Dispatcher = new();
        Assert.That(Dispatcher.HandlerCount, Is.Zero);

        Dispatcher.Register(TestEventHandler);
        Assert.That(Dispatcher.HandlerCount, Is.EqualTo(1));

        Dispatcher.Register(TestEventHandler);
        Assert.That(Dispatcher.HandlerCount, Is.EqualTo(1));

        Assert.DoesNotThrowAsync(async () => await Dispatcher.InvokeAsync(this).ConfigureAwait(false));
        Assert.That(Dispatcher.HandlerCount, Is.EqualTo(1));

        Dispatcher.Unregister(TestEventHandler);
        Assert.That(Dispatcher.HandlerCount, Is.Zero);

        Dispatcher.Unregister(TestEventHandler);
        Assert.That(Dispatcher.HandlerCount, Is.Zero);
    }

    private async Task TestEventHandler(object? sender, CancellationToken cancellationToken)
        => await Task.Delay(0, cancellationToken).ConfigureAwait(false);

    [Test]
    public void TestDispatcherNoNull()
    {
        AsyncEventDispatcher Dispatcher = new();

        _ = Assert.Throws<ArgumentNullException>(() => Dispatcher.Register(null!));
        _ = Assert.Throws<ArgumentNullException>(() => Dispatcher.Unregister(null!));
    }

    [Test]
    public async Task TestDispose()
    {
        TestService Service = new();
        TestClient Client = new();
        Assert.That(Client.Count, Is.Zero);

        Client.Init(Service);

        Assert.That(Service.HandlerCount, Is.EqualTo(1));

        await Service.Raise().ConfigureAwait(false);

        int LastCount = Client.Cleanup(Service);
        Assert.That(LastCount, Is.EqualTo(1));
        Assert.That(Client.Count, Is.EqualTo(LastCount));

        Assert.That(Service.HandlerCount, Is.Zero);

        Client.Init(Service);

        Assert.That(Service.HandlerCount, Is.EqualTo(1));

        Client.Dispose();
        GC.Collect();

        Assert.That(Service.HandlerCount, Is.EqualTo(1));

        await Service.Raise().ConfigureAwait(false);

        Assert.That(Service.HandlerCount, Is.Zero);
        Assert.That(Client.Count, Is.Zero);
    }

    [Test]
    public async Task TestRegisterUnregister()
    {
        TestService Service = new();
        TestClient Client1 = new();
        TestClient Client2 = new();

        Client1.Init(Service);
        Client2.Init(Service);

        Assert.That(Service.HandlerCount, Is.EqualTo(2));

        _ = Client2.Cleanup(Service);
        _ = Client1.Cleanup(Service);

        Client1.Init(Service);
        Client2.Init(Service);

        Client1.Dispose();
        GC.Collect();

        _ = Client2.Cleanup(Service);
        Client2.Init(Service);

        await Service.Raise().ConfigureAwait(false);

        Assert.That(Service.HandlerCount, Is.EqualTo(1));

        Client2.Dispose();
        GC.Collect();

        await Service.Raise().ConfigureAwait(false);

        Assert.That(Service.HandlerCount, Is.Zero);
    }

    private class TestService
    {
        public event AsyncEventHandler MyEvent
        {
            add => __myEvent.Register(value);
            remove => __myEvent.Unregister(value);
        }

        private readonly AsyncEventDispatcher __myEvent = new();

        private async Task RaiseMyEvent(CancellationToken cancellationToken = default)
            => await __myEvent.InvokeAsync(this, cancellationToken).ConfigureAwait(false);

        public async Task Raise()
        {
            Task task = RaiseMyEvent();
            await task.ConfigureAwait(false);
        }

        public int HandlerCount => __myEvent.HandlerCount;
    }

    private class TestClient : IDisposable
    {
        public void Init(TestService service)
        {
            Count = 0;
            service.MyEvent += OnMyEvent;
        }

        public int Cleanup(TestService service)
        {
            service.MyEvent -= OnMyEvent;
            return Count;
        }

        public int Count { get; private set; }

        private async Task OnMyEvent(object? sender, CancellationToken cancellationToken)
        {
            Count++;

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public void Dispose()
        {
        }
    }
}
