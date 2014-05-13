using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Protoreg;

namespace Elders.Cronus.Persistence.MSSQL
{
    public class MsSqlEventStorePlayer : IEventStorePlayer
    {
        private const string LoadEventsQueryTemplate = @"SELECT Events FROM {0} ORDER BY Revision OFFSET @offset ROWS FETCH NEXT {1} ROWS ONLY";

        private readonly string connectionString;

        private readonly ProtoregSerializer serializer;
        private readonly IMsSqlEventStoreTableNameStrategy tableNameStrategy;

        public MsSqlEventStorePlayer(IMsSqlEventStoreTableNameStrategy tableNameStrategy, ProtoregSerializer serializer, string connectionString)
        {
            this.tableNameStrategy = tableNameStrategy;
            this.connectionString = connectionString;
            this.serializer = serializer;
        }

        public IEnumerable<IEvent> GetEventsFromStart(int batchPerQuery = 1)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = String.Format(LoadEventsQueryTemplate, tableNameStrategy.GetEventsTableName(), batchPerQuery);
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@offset", 0);

                for (int i = 0; true; i++)
                {
                    command.Parameters[0].Value = i * batchPerQuery;
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows) break;

                        while (reader.Read())
                        {
                            var buffer = reader[0] as byte[];
                            EventBatchWraper wraper;
                            using (var stream = new MemoryStream(buffer))
                            {
                                wraper = (EventBatchWraper)serializer.Deserialize(stream);
                            }
                            foreach (IEvent @event in wraper.Events)
                            {
                                yield return @event;
                            }
                        }
                    }
                }
            }
        }
    }
}