using System;
using System.Collections.Generic;
using System.IO;
using Cassandra;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Persistence.Cassandra
{
    public class CassandraAggregateRepository : IAggregateRepository
    {
        private const string LoadAggregateEventsQueryTemplate = @"SELECT data FROM {0} WHERE id = ?;";

        private readonly IEventStorePersister persister;
        private readonly ISerializer serializer;
        private readonly ISession session;
        private readonly ICassandraEventStoreTableNameStrategy tableNameStrategy;

        private PreparedStatement loadAggregateEventsPreparedStatement;

        private static AggregateVersionService versionService = new AggregateVersionService();

        public CassandraAggregateRepository(ISession session, IEventStorePersister persister, ICassandraEventStoreTableNameStrategy tableNameStrategy, ISerializer serializer)
        {
            this.persister = persister;
            this.serializer = serializer;
            this.tableNameStrategy = tableNameStrategy;
            this.session = session;
            this.loadAggregateEventsPreparedStatement = session.Prepare(String.Format(LoadAggregateEventsQueryTemplate, tableNameStrategy.GetEventsTableName()));
        }



        //private List<IEvent> LoadAggregateEvents(Guid aggregateId)
        //{
        //    List<IEvent> events = new List<IEvent>();

        //    loadAggregateEventsPreparedStatement = loadAggregateEventsPreparedStatement ?? session.Prepare(String.Format(LoadAggregateEventsQueryTemplate, tableNameStrategy.GetEventsTableName()));

        //    BoundStatement bs = loadAggregateEventsPreparedStatement.Bind(aggregateId);
        //    var result = session.Execute(bs);
        //    foreach (var row in result.GetRows())
        //    {
        //        var data = row.GetValue<byte[]>("data");
        //        AggregateCommit commit;
        //        using (var stream = new MemoryStream(data))
        //        {
        //            commit = (AggregateCommit)serializer.Deserialize(stream);
        //        }
        //        events.AddRange(commit.Events);
        //    }
        //    return events;
        //}

        private List<AggregateCommit> LoadAggregateCommits(IAggregateRootId aggregateId)
        {
            List<AggregateCommit> events = new List<AggregateCommit>();
            BoundStatement bs = loadAggregateEventsPreparedStatement.Bind(aggregateId.Id);
            var result = session.Execute(bs);
            foreach (var row in result.GetRows())
            {
                var data = row.GetValue<byte[]>("data");
                using (var stream = new MemoryStream(data))
                {
                    events.Add((AggregateCommit)serializer.Deserialize(stream));
                }
            }
            return events;
        }

        public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {
            if (aggregateRoot.UncommittedEvents == null || aggregateRoot.UncommittedEvents.Count == 0)
                return;
            aggregateRoot.State.Version += 1;

            int reservedVersion = versionService.ReserveVersion(aggregateRoot.State.Id, aggregateRoot.State.Version);
            if (reservedVersion != aggregateRoot.State.Version)
            {
                throw new Exception("Retry command");
            }
            IDomainMessageCommit commit = new DomainMessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents);
            persister.Persist(new List<IDomainMessageCommit>() { commit });
        }

        public AR Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            var events = LoadAggregateCommits(id);
            AR aggregateRoot = AggregateRootFactory.Build<AR>(events);
            return aggregateRoot;
        }
    }
}