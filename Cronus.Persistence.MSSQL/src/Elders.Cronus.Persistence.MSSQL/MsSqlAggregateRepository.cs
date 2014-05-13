using System;
using System.Data.SqlClient;
using System.IO;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Protoreg;

namespace Elders.Cronus.Persistence.MSSQL
{
    public class MsSqlAggregateRepository : IAggregateRepository
    {
        private const string LoadAggregateStateQueryTemplate = @"SELECT TOP 1 AggregateState FROM {0} WHERE AggregateId=@aggregateId ORDER BY Version DESC";

        private readonly string connectionString;

        private readonly ProtoregSerializer serializer;

        private readonly IMsSqlEventStoreTableNameStrategy tableNameStrategy;

        private readonly IEventStorePersister persister;

        public MsSqlAggregateRepository(IEventStorePersister persister, IMsSqlEventStoreTableNameStrategy tableNameStrategy, ProtoregSerializer serializer, string connectionString)
        {
            this.persister = persister;
            this.tableNameStrategy = tableNameStrategy;
            this.connectionString = connectionString;
            this.serializer = serializer;
        }

        public void Save(IAggregateRoot aggregateRoot, ICommand command)
        {
            DomainMessageCommit domainMessageCommit = new DomainMessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents, command);
            persister.Persist(new System.Collections.Generic.List<DomainMessageCommit>() { domainMessageCommit });
        }

        public AR Update<AR>(IAggregateRootId aggregateId, ICommand command, Action<AR> update, Action<IAggregateRoot, ICommand> save = null) where AR : IAggregateRoot
        {
            var state = LoadAggregateState(aggregateId.Id);
            AR aggregateRoot = AggregateRootFactory.Build<AR>(state);
            update(aggregateRoot);
            if (save != null)
                save(aggregateRoot, command);
            else
                Save(aggregateRoot, command);
            return aggregateRoot;
        }

        private IAggregateRootState LoadAggregateState(Guid aggregateId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = String.Format(LoadAggregateStateQueryTemplate, tableNameStrategy.GetSnapshotsTableName());
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@aggregateId", aggregateId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var buffer = reader[0] as byte[];
                        IAggregateRootState state;
                        using (var stream = new MemoryStream(buffer))
                        {
                            state = (IAggregateRootState)serializer.Deserialize(stream);
                        }
                        return state;
                    }
                    else
                    {
                        return default(IAggregateRootState);
                    }
                }
            }
        }



        public AR Update<AR>(ICommand command, Action<AR> update, Action<IAggregateRoot, ICommand> save = null) where AR : IAggregateRoot
        {
            var state = LoadAggregateState(command.MetaAggregateId.Id);
            AR aggregateRoot = AggregateRootFactory.Build<AR>(state);
            update(aggregateRoot);
            if (save != null)
                save(aggregateRoot, command);
            else
                Save(aggregateRoot, command);
            return aggregateRoot;
        }
    }
}