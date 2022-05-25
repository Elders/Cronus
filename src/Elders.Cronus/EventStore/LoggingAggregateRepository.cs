using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Elders.Cronus.EventStore
{
    public sealed class LoggingAggregateRepository : IAggregateRepository
    {
        private readonly IAggregateRepository realDeal;
        private readonly ILogger<LoggingAggregateRepository> logger;

        public LoggingAggregateRepository(IAggregateRepository realDeal, ILogger<LoggingAggregateRepository> logger)
        {
            this.realDeal = realDeal;
            this.logger = logger;
        }

        public async Task<ReadResult<AR>> LoadAsync<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            using (logger.BeginScope(s => s
                                            .AddScope(Log.AggregateName, id.AggregateRootName)
                                            .AddScope(Log.AggregateId, id.Value)))
            {
                logger.Debug(() => "Loading aggregate...");
                return await realDeal.LoadAsync<AR>(id).ConfigureAwait(false);
            }
        }

        public async Task SaveAsync<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            using (logger.BeginScope(s => s
                                            .AddScope(Log.AggregateName, typeof(AR).Name)
                                            .AddScope(Log.AggregateId, aggregateRoot.State.Id.Value)))
            {
                await realDeal.SaveAsync<AR>(aggregateRoot).ConfigureAwait(false);
                logger.Debug(() => "Aggregate has been saved.");
            }
        }
    }
}
