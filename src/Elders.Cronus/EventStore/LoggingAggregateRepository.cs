using Microsoft.Extensions.Logging;

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

        public ReadResult<AR> Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            using (logger.BeginScope(s => s
                                            .AddScope(Log.AggregateName, id.AggregateRootName)
                                            .AddScope(Log.AggregateId, id.Value)))
            {
                logger.Debug(() => "Loading aggregate...");
                return realDeal.Load<AR>(id);
            }
        }

        public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            using (logger.BeginScope(s => s
                                            .AddScope(Log.AggregateName, typeof(AR).Name)
                                            .AddScope(Log.AggregateId, aggregateRoot.State.Id.Value)))
            {
                realDeal.Save<AR>(aggregateRoot);
                logger.Debug(() => "Aggregate has been saved.");
            }
        }
    }
}
