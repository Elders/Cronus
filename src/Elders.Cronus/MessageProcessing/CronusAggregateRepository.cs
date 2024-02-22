using System.Threading.Tasks;

namespace Elders.Cronus.MessageProcessing;

public sealed class CronusAggregateRepository : IAggregateRepository
{
    readonly IAggregateRepository aggregateRepository;

    public CronusAggregateRepository(IAggregateRepository repository)
    {
        this.aggregateRepository = repository;
    }

    public Task<ReadResult<AR>> LoadAsync<AR>(AggregateRootId id) where AR : IAggregateRoot
    {
        return aggregateRepository.LoadAsync<AR>(id);
    }

    public Task SaveAsync<AR>(AR aggregateRoot) where AR : IAggregateRoot
    {
        return aggregateRepository.SaveAsync(aggregateRoot);
    }
}
