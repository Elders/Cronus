using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Elders.Cronus.Tests.PublisherTests;

public class RetryableOperationAsyncTests
{
    [Fact]
    public async Task TryExecuteAsync_returns_value_on_first_success()
    {
        RetryPolicy policy = RetryableOperation.RetryPolicyFactory.CreateLinearRetryPolicy(3, TimeSpan.FromMilliseconds(1));

        int result = await RetryableOperation.TryExecuteAsync<int>(
            _ => Task.FromResult(42),
            policy);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task TryExecuteAsync_retries_until_success()
    {
        RetryPolicy policy = RetryableOperation.RetryPolicyFactory.CreateLinearRetryPolicy(5, TimeSpan.FromMilliseconds(1));
        int attempts = 0;

        int result = await RetryableOperation.TryExecuteAsync<int>(
            _ =>
            {
                attempts++;
                if (attempts < 3) throw new InvalidOperationException("transient");
                return Task.FromResult(99);
            },
            policy,
            getOperationInfo: () => "test-op");

        Assert.Equal(99, result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task TryExecuteAsync_propagates_cancellation()
    {
        RetryPolicy policy = RetryableOperation.RetryPolicyFactory.CreateLinearRetryPolicy(3, TimeSpan.FromSeconds(5));
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await RetryableOperation.TryExecuteAsync<int>(
                async ct =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), ct);
                    return 1;
                },
                policy,
                cancellationToken: cts.Token);
        });
    }
}
