using System.Collections.Generic;
using Elders.Cronus.EventStore;
using Elders.Cronus.Logging;

namespace Elders.Cronus.Migrations
{
    public abstract class MigrationRunnerBase<TDataFormat, TSourceEventStorePlayer, TTargetEventStore>
        where TSourceEventStorePlayer : IEventStorePlayer
        where TTargetEventStore : IEventStore
    {
        protected readonly IEventStorePlayer source;
        protected readonly IEventStore target;

        public MigrationRunnerBase(TSourceEventStorePlayer source, TTargetEventStore target)
        {
            this.source = source;
            this.target = target;
        }

        /// <summary>
        /// Applies the specified migrations to every <see cref="TDataFormat"/>
        /// </summary>
        /// <param name="migrations"></param>
        public abstract void Run(IEnumerable<IMigration<TDataFormat>> migrations);

    }
}
