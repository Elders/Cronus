using System;
using System.Collections.Generic;
using System.IO;
using Cassandra;
using Elders.Cronus.EventSourcing;
using Elders.Protoreg;

namespace Elders.Cronus.Persistence.Cassandra
{
    public class CassandraPersister : IEventStorePersister
    {

        private const string InsertEventQueryTemplate = @"INSERT INTO {0} (id,ts,rev,data) VALUES (?,?,?,?);";
        private const string InsertEventsBatchQueryTemplate = @"
BEGIN BATCH
  INSERT INTO {0} (id,ts,rev,data) VALUES (?,?,?,?);
  UPDATE {0}player SET events = events + ? WHERE date=?;
APPLY BATCH;";

        private PreparedStatement insertEventsPreparedStatement;
        private PreparedStatement insertEventsBatchPreparedStatement;

        private readonly ProtoregSerializer serializer;

        private readonly Session session;

        private readonly ICassandraEventStoreTableNameStrategy tableNameStrategy;

        public CassandraPersister(Session session, ICassandraEventStoreTableNameStrategy tableNameStrategy, ProtoregSerializer serializer)
        {
            this.tableNameStrategy = tableNameStrategy;
            this.session = session;
            this.serializer = serializer;
            insertEventsPreparedStatement = session.Prepare(String.Format(InsertEventQueryTemplate, tableNameStrategy.GetEventsTableName()));
            insertEventsBatchPreparedStatement = session.Prepare(String.Format(InsertEventsBatchQueryTemplate, tableNameStrategy.GetEventsTableName()));
        }

        public void Persist(List<DomainMessageCommit> commits)
        {
            foreach (var commit in commits)
            {
                AggregateCommit arCommit = new AggregateCommit(commit.State.Id, commit.State.Version, commit.Events);
                byte[] data = SerializeEvent(arCommit);
                session.Execute(insertEventsBatchPreparedStatement.Bind(arCommit.AggregateId, arCommit.Timestamp, arCommit.Revision, data, new List<byte[]>() { data }, DateTime.FromFileTimeUtc(arCommit.Timestamp).ToString("yyyyMMdd")));
            }
        }

        private byte[] SerializeEvent(AggregateCommit commit)
        {
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, commit);
                return stream.ToArray();
            }
        }

    }
}