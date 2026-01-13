namespace CSharpLatest.AsyncEventGenerator.Test;

using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpLatest.Events;
using NUnit.Framework;

[TestFixture]
internal class TestHandlerWithSenderAndArgs
{
    [Test]
    public void TestDispatcher()
    {
        AsyncEventDispatcher<string, EventArgs> Dispatcher = new();
        Assert.That(Dispatcher.HandlerCount, Is.Zero);

        Dispatcher.Register(TestEventHandler);
        Assert.That(Dispatcher.HandlerCount, Is.EqualTo(1));

        Dispatcher.Register(TestEventHandler);
        Assert.That(Dispatcher.HandlerCount, Is.EqualTo(1));

        Assert.DoesNotThrowAsync(async () => await Dispatcher.InvokeAsync(string.Empty, EventArgs.Empty).ConfigureAwait(false));
        Assert.That(Dispatcher.HandlerCount, Is.EqualTo(1));

        Dispatcher.Unregister(TestEventHandler);
        Assert.That(Dispatcher.HandlerCount, Is.Zero);

        Dispatcher.Unregister(TestEventHandler);
        Assert.That(Dispatcher.HandlerCount, Is.Zero);
    }

    private async Task TestEventHandler(object? sender, EventArgs args, CancellationToken cancellationToken)
        => await Task.Delay(0, cancellationToken).ConfigureAwait(false);

    [Test]
    public void TestDispatcherNoNull()
    {
        AsyncEventDispatcher<string, EventArgs> Dispatcher = new();

        _ = Assert.Throws<ArgumentNullException>(() => Dispatcher.Register(null!));
        _ = Assert.Throws<ArgumentNullException>(() => Dispatcher.Unregister(null!));
    }

    [Test]
    public async Task TestDispose()
    {
        TestService Service = new();

        await RunTestDisposeClient(Service).ConfigureAwait(false);
        GC.Collect();

        Assert.That(Service.HandlerCount, Is.EqualTo(1));

        await Service.Raise().ConfigureAwait(false);

        Assert.That(Service.HandlerCount, Is.Zero);
    }

    private static async Task RunTestDisposeClient(TestService service)
    {
        using TestClient Client = new();
        Assert.That(Client.Count, Is.Zero);

        Client.Init(service);

        Assert.That(service.HandlerCount, Is.EqualTo(1));

        await service.Raise().ConfigureAwait(false);

        int LastCount = Client.Cleanup(service);
        Assert.That(LastCount, Is.EqualTo(1));
        Assert.That(Client.Count, Is.EqualTo(LastCount));

        Assert.That(service.HandlerCount, Is.Zero);

        Client.Init(service);

        Assert.That(service.HandlerCount, Is.EqualTo(1));
    }

    [Test]
    public async Task TestRegisterUnregister()
    {
        TestService Service = new();

        await TestRegisterUnregisterClient(Service).ConfigureAwait(false);
        GC.Collect();

        await Service.Raise().ConfigureAwait(false);

        Assert.That(Service.HandlerCount, Is.Zero);
    }

    private static async Task TestRegisterUnregisterClient(TestService Service)
    {
        using TestClient Client2 = new();

        TestRegisterUnregisterClientNested(Service, Client2);
        GC.Collect();

        _ = Client2.Cleanup(Service);
        Client2.Init(Service);

        await Service.Raise().ConfigureAwait(false);

        Assert.That(Service.HandlerCount, Is.EqualTo(1));
    }

    private static void TestRegisterUnregisterClientNested(TestService Service, TestClient client2)
    {
        using TestClient Client1 = new();

        Client1.Init(Service);
        client2.Init(Service);

        Assert.That(Service.HandlerCount, Is.EqualTo(2));

        _ = client2.Cleanup(Service);
        _ = Client1.Cleanup(Service);

        Client1.Init(Service);
        client2.Init(Service);
    }

    private class TestService
    {
        public event AsyncEventHandler<string, EventArgs> MyEvent
        {
            add => __myEvent.Register(value);
            remove => __myEvent.Unregister(value);
        }

        private readonly AsyncEventDispatcher<string, EventArgs> __myEvent = new();

        private async Task RaiseMyEvent(CancellationToken cancellationToken = default)
            => await __myEvent.InvokeAsync(string.Empty, EventArgs.Empty, cancellationToken).ConfigureAwait(false);

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

        private async Task OnMyEvent(string? sender, EventArgs args, CancellationToken cancellationToken)
        {
            Count++;

            await Task.CompletedTask.ConfigureAwait(false);
        }

        public void Dispose()
        {
        }
    }
}
