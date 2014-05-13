using System;

namespace Elders.Cronus.Persistence.Cassandra
{
    public class CassandraEventStoreTableNameStrategy : ICassandraEventStoreTableNameStrategy
    {
        private readonly string eventsTableName;
        private readonly string snapshotsTableName;

        public CassandraEventStoreTableNameStrategy(string boundedContext)
        {
            eventsTableName = String.Format("{0}Events", boundedContext).ToLowerInvariant();
            snapshotsTableName = String.Format("{0}Snapshots", boundedContext).ToLowerInvariant();
        }

        public string GetEventsTableName()
        {
            return eventsTableName;
        }

        public string GetSnapshotsTableName()
        {
            return snapshotsTableName;
        }
    }
}