using System.Threading.Tasks;

namespace Elders.Cronus.MessageProcessing
{

    public sealed class CronusAggregateRepository : IAggregateRepository
    {
        // TODO: This class should be responsible for logging and building the headers.
        readonly IAggregateRepository aggregateRepository;

        public CronusAggregateRepository(IAggregateRepository repository)
        {
            this.aggregateRepository = repository;
        }

        public Task<ReadResult<AR>> LoadAsync<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            return aggregateRepository.LoadAsync<AR>(id);
        }

        public Task SaveAsync<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            return aggregateRepository.SaveAsync(aggregateRoot);
        }
    }
}
