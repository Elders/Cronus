﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elders.Cronus.EventStore;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Migrations
{
    public class DeleteEventStoreEvents<TSourceEventStorePlayer, TTargetEventStore> : MigrationRunnerBase<AggregateEventRaw, TSourceEventStorePlayer, IEventStore>
        where TSourceEventStorePlayer : IMigrationEventStorePlayer
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger(typeof(DeleteEventStoreEvents<,>));

        public DeleteEventStoreEvents(TSourceEventStorePlayer source, EventStoreFactory factory) : base(source, factory.GetEventStore()) { }

        public override async Task RunAsync(IEnumerable<IMigration<AggregateEventRaw>> migrations)
        {
            try
            {
                PlayerOperator @operator = new PlayerOperator()
                {
                    OnLoadAsync = async @event =>
                    {
                        foreach (IMigration<AggregateEventRaw> migration in migrations)
                        {
                            if (migration.ShouldApply(@event))
                            {
                                await target.DeleteAsync(@event).ConfigureAwait(false);
                            }
                        }
                    }
                };

                PlayerOptions options = options = new PlayerOptions();

                await source.EnumerateEventStore(@operator, options).ConfigureAwait(false);

            }
            catch (System.Exception ex)
            {
                logger.ErrorException(ex, () => $"Something boom bam while runnning migration.");
            }

        }
        private static bool ForSomeReasonTheAggregateCommitHasBeenDeleted(AggregateCommit aggregateCommit)
        {
            return aggregateCommit is null || aggregateCommit.Events.Any() == false;
        }
    }
}