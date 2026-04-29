using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Elders.Cronus.Tests.PublisherTests;

public class AsyncStressTests
{
    private sealed class TestMessage : IMessage
    {
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    }

    private sealed class CountingPublisher : IPublisher<TestMessage>
    {
        public int CallCount;

        public Task<bool> PublishAsync(TestMessage message, Dictionary<string, string> messageHeaders = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref CallCount);
            return Task.FromResult(true);
        }

        public Task<bool> PublishAsync(TestMessage message, DateTime publishAt, Dictionary<string, string> messageHeaders = null, CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task<bool> PublishAsync(TestMessage message, TimeSpan publishAfter, Dictionary<string, string> messageHeaders = null, CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task<bool> PublishAsync(byte[] messageRaw, Type messageType, string tenant, Dictionary<string, string> messageHeaders = null, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class SlowPublisher : IPublisher<TestMessage>
    {
        public async Task<bool> PublishAsync(TestMessage message, Dictionary<string, string> messageHeaders = null, CancellationToken cancellationToken = default)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
            return true;
        }

        public Task<bool> PublishAsync(TestMessage message, DateTime publishAt, Dictionary<string, string> messageHeaders = null, CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task<bool> PublishAsync(TestMessage message, TimeSpan publishAfter, Dictionary<string, string> messageHeaders = null, CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task<bool> PublishAsync(byte[] messageRaw, Type messageType, string tenant, Dictionary<string, string> messageHeaders = null, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    [Fact]
    public async Task ParallelFanout_1000_concurrent_publishes_no_deadlock()
    {
        var pub = new CountingPublisher();
        var tasks = Enumerable.Range(0, 1000).Select(_ => pub.PublishAsync(new TestMessage())).ToArray();
        var sw = Stopwatch.StartNew();
        var results = await Task.WhenAll(tasks);
        sw.Stop();

        Assert.Equal(1000, pub.CallCount);
        Assert.All(results, r => Assert.True(r));
        Assert.True(sw.ElapsedMilliseconds < 5000, $"1000 publishes took {sw.ElapsedMilliseconds}ms (expected <5s)");
    }

    [Fact]
    public async Task CancellationToken_propagates_during_in_flight_publish()
    {
        var slowPub = new SlowPublisher();
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        var sw = Stopwatch.StartNew();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await slowPub.PublishAsync(new TestMessage(), cancellationToken: cts.Token);
        });

        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 200, $"Cancellation took {sw.ElapsedMilliseconds}ms (expected <200ms)");
    }
}
