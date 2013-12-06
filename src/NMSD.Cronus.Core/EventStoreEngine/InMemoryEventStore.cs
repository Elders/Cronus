using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Cronus.Core.Eventing;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Snapshotting;
using Protoreg;

namespace NMSD.Cronus.Core.EventStoreEngine
{
    [DataContract(Name = "987a7bed-7689-4c08-b610-9a802d306215")]
    public class Wraper
    {
        Wraper() { }

        public Wraper(List<object> events)
        {
            Events = events;
        }

        [DataMember(Order = 1)]
        public List<object> Events { get; private set; }

    }
    public class InMemoryEventStore : ISnapShotter
    {
        const string LoadAggregateStateQueryTemplate = @"SELECT TOP 1 AggregateState FROM {0}Snapshots WHERE AggregateId=@aggregateId ORDER BY Version DESC";

        const string LoadEventsQueryTemplate = @"SELECT Events FROM {0}Events ORDER BY Revision OFFSET @offset ROWS FETCH NEXT {1} ROWS ONLY";

        private readonly string connectionString;

        private readonly ProtoregSerializer serializer;

        public InMemoryEventStore(string connectionString, ProtoregSerializer serializer)
        {
            this.connectionString = connectionString;
            this.serializer = serializer;
        }

        public IEnumerable<IEvent> GetEventsFromStart(string boundedContext, int batchPerQuery = 1)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = String.Format(LoadEventsQueryTemplate, boundedContext, batchPerQuery);
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
                            Wraper wraper;
                            using (var stream = new MemoryStream(buffer))
                            {
                                wraper = (Wraper)serializer.Deserialize(stream);
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

        public IAggregateRootState LoadAggregateState(string boundedContext, Guid aggregateId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = String.Format(LoadAggregateStateQueryTemplate, boundedContext);
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

        public void Persist(List<IEvent> events)
        {
            byte[] buffer = SerializeEvents(events);

            DataTable eventsTable = CreateInMemoryTableForEvents();
            var row = eventsTable.NewRow();
            row[1] = buffer;
            row[2] = events.Count;
            row[3] = DateTime.UtcNow;
            eventsTable.Rows.Add(row);

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
            {
                string boundedContext = MessagingHelper.GetBoundedContext(events.First().GetType());
                bulkCopy.DestinationTableName = String.Format("dbo.{0}Events", boundedContext);
                try
                {
                    bulkCopy.WriteToServer(eventsTable, DataRowState.Added);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void TakeSnapshot(IAggregateRootState state)
        {
            byte[] buffer = SerializeAggregateState(state);

            DataTable dt = CreateInMemoryTableForSnapshots();

            var row = dt.NewRow();
            row[0] = state.Version;
            row[1] = state.Id.Id;
            row[2] = buffer;
            row[3] = DateTime.UtcNow;
            dt.Rows.Add(row);

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
            {
                string boundedContext = MessagingHelper.GetBoundedContext(state.GetType());
                bulkCopy.DestinationTableName = String.Format("dbo.{0}Snapshots", boundedContext);

                try
                {
                    bulkCopy.WriteToServer(dt, DataRowState.Added);
                }
                catch (Exception ex)
                {
                    throw new AggregateStateFirstLevelConcurrencyException("", ex);
                }
            }
        }

        private static DataTable CreateInMemoryTableForEvents()
        {
            DataTable uncommittedEvents = new DataTable();

            DataColumn revision = new DataColumn();
            revision.DataType = typeof(int);
            revision.ColumnName = "Revision";
            revision.AutoIncrement = true;
            revision.Unique = true;
            uncommittedEvents.Columns.Add(revision);

            DataColumn events = new DataColumn();
            events.DataType = typeof(byte[]);
            events.ColumnName = "Events";
            uncommittedEvents.Columns.Add(events);

            DataColumn eventsCount = new DataColumn();
            eventsCount.DataType = typeof(uint);
            eventsCount.ColumnName = "EventsCount";
            uncommittedEvents.Columns.Add(eventsCount);

            DataColumn timestamp = new DataColumn();
            timestamp.DataType = typeof(DateTime);
            timestamp.ColumnName = "Timestamp";
            uncommittedEvents.Columns.Add(timestamp);

            DataColumn[] keys = new DataColumn[1];
            keys[0] = revision;
            uncommittedEvents.PrimaryKey = keys;

            return uncommittedEvents;
        }

        private static DataTable CreateInMemoryTableForSnapshots()
        {
            DataTable uncommittedState = new DataTable();

            DataColumn version = new DataColumn();
            version.DataType = typeof(int);
            version.ColumnName = "Version";
            uncommittedState.Columns.Add(version);

            DataColumn aggregateId = new DataColumn();
            aggregateId.DataType = typeof(Guid);
            aggregateId.ColumnName = "AggregateId";
            uncommittedState.Columns.Add(aggregateId);

            DataColumn events = new DataColumn();
            events.DataType = typeof(byte[]);
            events.ColumnName = "AggregateState";
            uncommittedState.Columns.Add(events);

            DataColumn timestamp = new DataColumn();
            timestamp.DataType = typeof(DateTime);
            timestamp.ColumnName = "Timestamp";
            uncommittedState.Columns.Add(timestamp);

            DataColumn[] keys = new DataColumn[2];
            keys[0] = version;
            keys[1] = aggregateId;
            uncommittedState.PrimaryKey = keys;

            return uncommittedState;
        }

        private byte[] SerializeAggregateState(IAggregateRootState aggregateRootState)
        {
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, aggregateRootState);
                return stream.ToArray();
            }
        }

        private byte[] SerializeEvents(List<IEvent> events)
        {
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, new Wraper(events.Cast<object>().ToList()));
                return stream.ToArray();
            }
        }

    }
}
