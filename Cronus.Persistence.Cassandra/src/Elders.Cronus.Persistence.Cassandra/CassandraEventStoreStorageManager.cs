using System;
using Cassandra;
using Elders.Cronus.EventSourcing;

namespace Elders.Cronus.Persistence.Cassandra
{
    public class CassandraEventStoreStorageManager : IEventStoreStorageManager
    {
        private const string CreateKeySpaceTemplate = @"CREATE KEYSPACE IF NOT EXISTS {0} WITH replication = {{'class':'SimpleStrategy', 'replication_factor':1}};";
        private const string CreateEventsTableTemplate = @"CREATE TABLE IF NOT EXISTS ""{0}"" (id uuid, ts bigint, rev int, data blob, PRIMARY KEY (id,ts,rev)) WITH compression = {{ 'sstable_compression' : '' }};";
        private const string CreateReplayEventsTableTemplate = @"CREATE TABLE IF NOT EXISTS ""{0}player"" (date text, events list<blob>, PRIMARY KEY (date)) WITH compression = {{ 'sstable_compression' : '' }};";
        //private const string CreateSnapshotsTableTemplate = @"CREATE TABLE IF NOT EXISTS ""{0}"" (id uuid, ver int, ts bigint, data blob, PRIMARY KEY (id,ver));";

        private readonly Session session;
        private readonly ICassandraEventStoreTableNameStrategy tableNameStrategy;

        public CassandraEventStoreStorageManager(Session session, ICassandraEventStoreTableNameStrategy tableNameStrategy)
        {
            this.session = session;
            this.tableNameStrategy = tableNameStrategy;
        }

        public void CreateEventsStorage()
        {
            var createEventsTable = String.Format(CreateEventsTableTemplate, tableNameStrategy.GetEventsTableName()).ToLower();
            session.Execute(createEventsTable);

            var createEventsPlayerTable = String.Format(CreateReplayEventsTableTemplate, tableNameStrategy.GetEventsTableName()).ToLower();
            session.Execute(createEventsPlayerTable);
        }

        public void CreateStorage()
        {
            var createKeySpaceQuery = String.Format(CreateKeySpaceTemplate, session.Keyspace);
            session.Execute(createKeySpaceQuery);

            CreateEventsStorage();
            CreateSnapshotsStorage();
        }

        public void CreateSnapshotsStorage()
        {
            //var createSnapshotsTable = String.Format(CreateSnapshotsTableTemplate, tableNameStrategy.GetSnapshotsTableName()).ToLower();
            //session.Execute(createSnapshotsTable);
        }
    }
}
