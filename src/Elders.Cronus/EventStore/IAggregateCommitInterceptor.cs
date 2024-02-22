using System.Threading.Tasks;

namespace Elders.Cronus.EventStore;

public interface IAggregateCommitInterceptor
{
    Task<AggregateCommit> OnAppendingAsync(AggregateCommit origin);

    Task OnAppendAsync(AggregateCommit origin);
}
