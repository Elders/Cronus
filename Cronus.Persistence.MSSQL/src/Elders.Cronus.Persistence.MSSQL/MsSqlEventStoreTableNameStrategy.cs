using System;

namespace Elders.Cronus.Persistence.MSSQL
{
    public class MsSqlEventStoreTableNameStrategy : IMsSqlEventStoreTableNameStrategy
    {
        private readonly string eventsTableName;
        private readonly string snapshotsTableName;

        public MsSqlEventStoreTableNameStrategy(string boundedContext)
        {
            eventsTableName = String.Format("dbo.{0}Events", boundedContext);
            snapshotsTableName = String.Format("dbo.{0}Snapshots", boundedContext);
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