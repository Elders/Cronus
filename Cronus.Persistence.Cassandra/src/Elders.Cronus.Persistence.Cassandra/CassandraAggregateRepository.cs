using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cassandra;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Protoreg;
using System.Linq;

namespace Elders.Cronus.Persistence.Cassandra
{
    public class CassandraAggregateRepository : IAggregateRepository
    {
        private readonly IEventStorePersister persister;
        private readonly ProtoregSerializer serializer;
        private readonly Session session;
        private readonly ICassandraEventStoreTableNameStrategy tableNameStrategy;

        private static AggregateVersionService versionService = new AggregateVersionService();

        public CassandraAggregateRepository(Session session, IEventStorePersister persister, ICassandraEventStoreTableNameStrategy tableNameStrategy, ProtoregSerializer serializer)
        {
            this.persister = persister;
            this.serializer = serializer;
            this.tableNameStrategy = tableNameStrategy;
            this.session = session;
            this.loadAggregateEventsPreparedStatement = session.Prepare(String.Format(LoadAggregateEventsQueryTemplate, tableNameStrategy.GetEventsTableName()));
        }

        public void Save(IAggregateRoot aggregateRoot, ICommand command)
        {
            if (aggregateRoot.UncommittedEvents == null || aggregateRoot.UncommittedEvents.Count == 0)
                return;
            aggregateRoot.State.Version += 1;

            int reservedVersion = versionService.ReserveVersion(aggregateRoot.State.Id, aggregateRoot.State.Version);
            if (reservedVersion != aggregateRoot.State.Version)
            {
                throw new Exception("Retry command");
            }
            DomainMessageCommit commit = new DomainMessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents, command);
            persister.Persist(new List<DomainMessageCommit>() { commit });
        }

        public AR Update<AR>(IAggregateRootId aggregateId, ICommand command, Action<AR> update, Action<IAggregateRoot, ICommand> save = null) where AR : IAggregateRoot
        {
            throw new NotImplementedException();
        }

        private const string LoadAggregateEventsQueryTemplate = @"SELECT data FROM {0} WHERE id = ?;";

        private PreparedStatement loadAggregateEventsPreparedStatement;

        private List<IEvent> LoadAggregateEvents(Guid aggregateId)
        {
            List<IEvent> events = new List<IEvent>();

            loadAggregateEventsPreparedStatement = loadAggregateEventsPreparedStatement ?? session.Prepare(String.Format(LoadAggregateEventsQueryTemplate, tableNameStrategy.GetEventsTableName()));

            BoundStatement bs = loadAggregateEventsPreparedStatement.Bind(aggregateId);
            var result = session.Execute(bs);
            foreach (var row in result.GetRows())
            {
                var data = row.GetValue<byte[]>("data");
                AggregateCommit commit;
                using (var stream = new MemoryStream(data))
                {
                    commit = (AggregateCommit)serializer.Deserialize(stream);
                }
                events.AddRange(commit.Events);
            }
            return events;
        }

        private List<AggregateCommit> LoadAggregateCommits(Guid aggregateId)
        {
            List<AggregateCommit> events = new List<AggregateCommit>();
            BoundStatement bs = loadAggregateEventsPreparedStatement.Bind(aggregateId);
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


        public AR Update<AR>(ICommand command, Action<AR> update, Action<IAggregateRoot, ICommand> save = null) where AR : IAggregateRoot
        {
            var events = LoadAggregateCommits(command.MetaAggregateId.Id);
            AR aggregateRoot = AggregateRootFactory.Build<AR>(events);
            update(aggregateRoot);
            if (save != null)
                save(aggregateRoot, command);
            else
                Save(aggregateRoot, command);
            return aggregateRoot;

        }
    }
}