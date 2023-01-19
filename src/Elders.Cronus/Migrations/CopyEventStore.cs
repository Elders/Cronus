using System.Collections.Generic;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations
{
    public class CopyEventStore<TSourceEventStorePlayer, TTargetEventStore> : MigrationRunnerBase<AggregateEventRaw, TSourceEventStorePlayer, TTargetEventStore>
        where TSourceEventStorePlayer : IEventStorePlayer
        where TTargetEventStore : IEventStore
    {
        private readonly ILogger logger;

        public CopyEventStore(TSourceEventStorePlayer source, TTargetEventStore target, ILogger logger) : base(source, target)
        {
            this.logger = logger;
        }

        public override async Task RunAsync(IEnumerable<IMigration<AggregateEventRaw>> migrations)
        {
            PlayerOperator @operator = new PlayerOperator()
            {
                OnLoadAsync = raw => target.AppendAsync(raw)
            };

            PlayerOptions playerOptions = new PlayerOptions();
            await source.EnumerateEventStore(@operator, playerOptions);
        }
    }
}
