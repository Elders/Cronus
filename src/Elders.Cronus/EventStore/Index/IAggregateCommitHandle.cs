using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index;

public interface IAggregateCommitHandle<in T>
    where T : AggregateCommit
{
    Task HandleAsync(T @event);
}
