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
        public CopyEventStore(TSourceEventStorePlayer source, TTargetEventStore target, ILogger logger) : base(source, target)
        {
        }

        public override async Task RunAsync(IEnumerable<IMigration<AggregateEventRaw>> migrations)
        {
            PlayerOperator @operator = new PlayerOperator()
            {
                OnLoadAsync = target.AppendAsync
            };

            PlayerOptions playerOptions = new PlayerOptions();
            await source.EnumerateEventStore(@operator, playerOptions);
        }
    }
}
