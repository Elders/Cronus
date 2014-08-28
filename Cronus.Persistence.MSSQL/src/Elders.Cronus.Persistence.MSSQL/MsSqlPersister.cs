using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Serializer;
using Elders.Protoreg;

namespace Elders.Cronus.Persistence.MSSQL
{
    public class MsSqlPersister : IEventStorePersister
    {
        private readonly string connectionString;
        private readonly ISerializer serializer;
        private readonly IMsSqlEventStoreTableNameStrategy tableNameStrategy;

        public MsSqlPersister(IMsSqlEventStoreTableNameStrategy tableNameStrategy, ISerializer serializer, string connectionString)
        {
            this.connectionString = connectionString;
            this.tableNameStrategy = tableNameStrategy;
            this.serializer = serializer;
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
                serializer.Serialize(stream, new EventBatchWraper(events.Cast<object>().ToList()));
                return stream.ToArray();
            }
        }

        //private void ValidateSnapshotsBoundedContext(IList<IAggregateRootState> states)
        //{
        //    StringBuilder errors = new StringBuilder();
        //    ISet<Type> wrongStateTypes = new HashSet<Type>();
        //    foreach (var @event in states)
        //    {
        //        var eventType = @event.GetType();
        //        string eventBC = eventType.GetBoundedContext().BoundedContextName;
        //        if (String.Compare(BoundedContext, eventBC, true, CultureInfo.InvariantCulture) != 0)
        //            wrongStateTypes.Add(eventType);
        //    }

        //    if (wrongStateTypes.Count > 0)
        //    {
        //        foreach (Type et in wrongStateTypes)
        //        {
        //            errors.AppendLine();
        //            errors.Append(et.FullName);
        //        }
        //        string errorMessage = String.Format("The following states do not belong to the '{0} bounded context. {1}", BoundedContext, errors.ToString());
        //        throw new Exception(errorMessage);
        //    }
        //}

        public void Persist(List<IDomainMessageCommit> commits)
        {
            //ValidateSnapshotsBoundedContext
            //#if DEBUG
            //            ValidateEventsBoundedContext(events);
            //#endif

            DataTable eventsTable = CreateInMemoryTableForEvents();
            DataTable snapshotsTable = CreateInMemoryTableForSnapshots();
            try
            {
                foreach (var commit in commits)
                {
                    //  Events
                    byte[] eventsBuffer = SerializeEvents(commit.Events);
                    var eventsRow = eventsTable.NewRow();
                    eventsRow[1] = eventsBuffer;
                    eventsRow[2] = commit.Events.Count;
                    eventsRow[3] = DateTime.UtcNow;
                    eventsTable.Rows.Add(eventsRow);

                    //  Snapshots
                    byte[] buffer = SerializeAggregateState(commit.State);
                    var snapshotRow = snapshotsTable.NewRow();
                    snapshotRow[0] = commit.State.Version;
                    snapshotRow[1] = commit.State.Id.Id;
                    snapshotRow[2] = buffer;
                    snapshotRow[3] = DateTime.UtcNow;
                    snapshotsTable.Rows.Add(snapshotRow);
                }
            }
            catch (Exception ex)
            {
                eventsTable.Clear();
                snapshotsTable.Clear();
                throw new AggregateStateFirstLevelConcurrencyException("", ex);
            }

            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = tableNameStrategy.GetSnapshotsTableName();
                    bulkCopy.WriteToServer(snapshotsTable, DataRowState.Added);
                }
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = tableNameStrategy.GetEventsTableName();
                    bulkCopy.WriteToServer(eventsTable, DataRowState.Added);
                }
            }
            catch (Exception ex)
            {
                throw new AggregateStateFirstLevelConcurrencyException("", ex);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                else
                {
                    throw new Exception("EventStore bulk persist connection is in strange state.");
                }
            }
        }
    }
}
