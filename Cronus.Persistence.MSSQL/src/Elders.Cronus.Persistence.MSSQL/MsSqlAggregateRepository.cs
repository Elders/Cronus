using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Persistence.MSSQL
{
    public class MsSqlAggregateRepository : IAggregateRepository
    {
        private const string LoadAggregateStateQueryTemplate = @"SELECT TOP 1 AggregateState FROM {0} WHERE AggregateId=@aggregateId ORDER BY Version DESC";

        private readonly string connectionString;

        private readonly ISerializer serializer;

        private readonly IMsSqlEventStoreTableNameStrategy tableNameStrategy;

        private readonly IEventStorePersister persister;


        public MsSqlAggregateRepository(IEventStorePersister persister, IMsSqlEventStoreTableNameStrategy tableNameStrategy, ISerializer serializer, string connectionString)
        {
            this.persister = persister;
            this.tableNameStrategy = tableNameStrategy;
            this.connectionString = connectionString;
            this.serializer = serializer;
        }

        private IAggregateRootState LoadAggregateState(IAggregateRootId aggregateId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = String.Format(LoadAggregateStateQueryTemplate, tableNameStrategy.GetSnapshotsTableName());
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@aggregateId", aggregateId.Id);

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

        public void Save<AR>(AR aggregateRoot) where AR : IAggregateRoot
        {

            if (ReferenceEquals(null, aggregateRoot)) throw new ArgumentNullException("aggregateRoot");
            if (aggregateRoot.UncommittedEvents.Count > 0)
            {
                var dmc = new DomainMessageCommit(aggregateRoot.State, aggregateRoot.UncommittedEvents);
                persister.Persist(new List<IDomainMessageCommit>() { dmc });
            }
        }

        public AR Load<AR>(IAggregateRootId id) where AR : IAggregateRoot
        {
            var state = LoadAggregateState(id);
            AR aggregateRoot = AggregateRootFactory.Build<AR>(state);
            return aggregateRoot;
        }
    }
}
