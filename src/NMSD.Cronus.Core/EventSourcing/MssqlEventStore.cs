using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.Messaging;
using NMSD.Protoreg;
using System.Text;
using System.Globalization;
using NMSD.Cronus.Core.DomainModelling;

namespace NMSD.Cronus.Core.EventSourcing
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
    public class MssqlEventStore : IAggregateRepository, IEventStore
    {
        const string DeleteAggregateStatesQueryTemplate = @"DELETE {0} WHERE [Timestamp]<@timestamp AND ({1})";

        const string DeleteAggregateStatesWhereTemplate = @"AggregateId=@aggregateId{0} AND [Version]<@version{0}";

        const string LoadAggregateStateQueryTemplate = @"SELECT TOP 1 AggregateState FROM {0} WHERE AggregateId=@aggregateId ORDER BY Version DESC";

        const string LoadEventsQueryTemplate = @"SELECT Events FROM {0} ORDER BY Revision OFFSET @offset ROWS FETCH NEXT {1} ROWS ONLY";

        private readonly string boundedContext;

        private readonly string connectionString;

        ConcurrentDictionary<Type, Tuple<string, string>> eventsInfo = new ConcurrentDictionary<Type, Tuple<string, string>>();

        private readonly string eventsTableName;

        private readonly ProtoregSerializer serializer;

        ConcurrentDictionary<Type, Tuple<string, string>> snapshotsInfo = new ConcurrentDictionary<Type, Tuple<string, string>>();

        private readonly string snapshotsTableName;

        public MssqlEventStore(string boundedContext, string connectionString, ProtoregSerializer serializer)
        {
            this.boundedContext = boundedContext;
            eventsTableName = String.Format("dbo.{0}Events", boundedContext);
            snapshotsTableName = String.Format("dbo.{0}Snapshots", boundedContext);
            this.connectionString = connectionString;
            this.serializer = serializer;
        }

        public void CloseConnection(SqlConnection conn)
        {
            conn.Close();
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

        public IAggregateRootState LoadAggregateState(Guid aggregateId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = String.Format(LoadAggregateStateQueryTemplate, snapshotsTableName);
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

        public SqlConnection OpenConnection()
        {
            var conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        public void Persist(List<IEvent> events, SqlConnection connection)
        {
            if (events == null) throw new ArgumentNullException("events");
            if (events.Count == 0) return;

#if DEBUG
            ValidateEventsBoundedContext(events);
#endif

            byte[] buffer = SerializeEvents(events);

            DataTable eventsTable = CreateInMemoryTableForEvents();
            var row = eventsTable.NewRow();
            row[1] = buffer;
            row[2] = events.Count;
            row[3] = DateTime.UtcNow;
            eventsTable.Rows.Add(row);
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.DestinationTableName = eventsTableName;
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

        public void TakeSnapshot(List<IAggregateRootState> states, SqlConnection connection)
        {
#if DEBUG
            ValidateSnapshotsBoundedContext(states);
#endif

            DataTable dt = CreateInMemoryTableForSnapshots();
            foreach (var state in states)
            {
                byte[] buffer = SerializeAggregateState(state);
                var row = dt.NewRow();
                row[0] = state.Version;
                row[1] = state.Id.Id;
                row[2] = buffer;
                row[3] = DateTime.UtcNow;
                dt.Rows.Add(row);
            }

            // using (var tx = connection.BeginTransaction(IsolationLevel.Snapshot))
            {
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = snapshotsTableName;
                    try
                    {
                        //var statesDeleteCommand = PrepareAggregateStateDeleteCommand(states);
                        //statesDeleteCommand.Connection = connection;
                        //statesDeleteCommand.Transaction = tx;
                        //statesDeleteCommand.ExecuteNonQuery();

                        bulkCopy.BatchSize = 100;
                        bulkCopy.WriteToServer(dt, DataRowState.Added);
                        // tx.Commit();

                    }
                    catch (Exception ex)
                    {
                        // tx.Rollback();
                        throw new AggregateStateFirstLevelConcurrencyException("", ex);
                    }
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

        SqlCommand PrepareAggregateStateDeleteCommand(List<IAggregateRootState> states)
        {
            var deleteCmd = new SqlCommand();
            StringBuilder deleteWhereClauseBuilder = new StringBuilder();
            for (int i = 0; i < states.Count; i++)
            {
                deleteWhereClauseBuilder.AppendFormat(DeleteAggregateStatesWhereTemplate, i);
                deleteCmd.Parameters.AddWithValue("@aggregateId" + i, states[i].Id.Id);
                deleteCmd.Parameters.AddWithValue("@version" + i, states[i].Version - 5);

                if (i < states.Count - 1)
                    deleteWhereClauseBuilder.Append(" OR ");
            }
            deleteCmd.Parameters.AddWithValue("@timestamp", DateTime.UtcNow.AddHours(-1));
            deleteCmd.CommandText = String.Format(DeleteAggregateStatesQueryTemplate, snapshotsTableName, deleteWhereClauseBuilder.ToString());

            return deleteCmd;
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

        private void ValidateEventsBoundedContext(IList<IEvent> events)
        {
            StringBuilder errors = new StringBuilder();
            ISet<Type> wrongEventTypes = new HashSet<Type>();
            foreach (var @event in events)
            {
                var eventType = @event.GetType();
                string eventBC = MessageInfo.GetBoundedContext(eventType);
                if (String.Compare(boundedContext, eventBC, true, CultureInfo.InvariantCulture) != 0)
                    wrongEventTypes.Add(eventType);
            }

            if (wrongEventTypes.Count > 0)
            {
                foreach (Type et in wrongEventTypes)
                {
                    errors.AppendLine();
                    errors.Append(et.FullName);
                }
                string errorMessage = String.Format("The following events do not belong to the '{0} bounded context. {1}", boundedContext, errors.ToString());
                throw new Exception(errorMessage);
            }
        }

        private void ValidateSnapshotsBoundedContext(IList<IAggregateRootState> states)
        {
            StringBuilder errors = new StringBuilder();
            ISet<Type> wrongStateTypes = new HashSet<Type>();
            foreach (var @event in states)
            {
                var eventType = @event.GetType();
                string eventBC = MessageInfo.GetBoundedContext(eventType);
                if (String.Compare(boundedContext, eventBC, true, CultureInfo.InvariantCulture) != 0)
                    wrongStateTypes.Add(eventType);
            }

            if (wrongStateTypes.Count > 0)
            {
                foreach (Type et in wrongStateTypes)
                {
                    errors.AppendLine();
                    errors.Append(et.FullName);
                }
                string errorMessage = String.Format("The following states do not belong to the '{0} bounded context. {1}", boundedContext, errors.ToString());
                throw new Exception(errorMessage);
            }
        }


        public AR Load<AR>(IAggregateRootId aggregateId) where AR : IAggregateRoot
        {
            var state = LoadAggregateState(aggregateId.Id);
            AR aggregateRoot = AggregateRootFactory.Build<AR>(state);
            return aggregateRoot;
        }


        public void Save(IAggregateRoot aggregateRoot)
        {
            throw new NotImplementedException();
        }



        IEventStream IEventStore.OpenStream()
        {
            var connection = OpenConnection();
            return new MssqlStream(connection);
        }

        public void Commit(MssqlStream stream)
        {
            if (stream.Events.Count == 0)
                return;
            Persist(stream.Events, stream.Connection);
            TakeSnapshot(stream.Snapshots, stream.Connection);
        }

        void IEventStore.Commit(IEventStream stream)
        {
            Commit(stream as MssqlStream);
        }
    }

    public static class MeasureExecutionTime
    {
        public static string Start(System.Action action)
        {
            string result = string.Empty;

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            action();
            stopWatch.Stop();
            System.TimeSpan ts = stopWatch.Elapsed;
            result = System.String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            return result;
        }

        public static string Start(System.Action action, int repeat, bool showTicksInfo = false)
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < repeat; i++)
            {
                action();
            }
            stopWatch.Stop();
            System.TimeSpan total = stopWatch.Elapsed;
            System.TimeSpan average = new System.TimeSpan(stopWatch.Elapsed.Ticks / repeat);

            System.Text.StringBuilder perfResultsBuilder = new System.Text.StringBuilder();
            perfResultsBuilder.AppendLine("--------------------------------------------------------------");
            perfResultsBuilder.AppendFormat("  Total Time => {0}\r\nAverage Time => {1}", Align(total), Align(average));
            perfResultsBuilder.AppendLine();
            perfResultsBuilder.AppendLine("--------------------------------------------------------------");
            if (showTicksInfo)
                perfResultsBuilder.AppendLine(TicksInfo());
            return perfResultsBuilder.ToString();
        }

        static string Align(System.TimeSpan interval)
        {
            string intervalStr = interval.ToString();
            int pointIndex = intervalStr.IndexOf(':');

            pointIndex = intervalStr.IndexOf('.', pointIndex);
            if (pointIndex < 0) intervalStr += "        ";
            return intervalStr;
        }

        static string TicksInfo()
        {
            System.Text.StringBuilder ticksInfoBuilder = new System.Text.StringBuilder("\r\n\r\n");
            ticksInfoBuilder.AppendLine("Ticks Info");
            ticksInfoBuilder.AppendLine("--------------------------------------------------------------");
            const string numberFmt = "{0,-22}{1,18:N0}";
            const string timeFmt = "{0,-22}{1,26}";

            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "Field", "Value"));
            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "-----", "-----"));

            // Display the maximum, minimum, and zero TimeSpan values.
            ticksInfoBuilder.AppendLine(System.String.Format(timeFmt, "Maximum TimeSpan", Align(System.TimeSpan.MaxValue)));
            ticksInfoBuilder.AppendLine(System.String.Format(timeFmt, "Minimum TimeSpan", Align(System.TimeSpan.MinValue)));
            ticksInfoBuilder.AppendLine(System.String.Format(timeFmt, "Zero TimeSpan", Align(System.TimeSpan.Zero)));
            ticksInfoBuilder.AppendLine();

            // Display the ticks-per-time-unit fields.
            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "Ticks per day", System.TimeSpan.TicksPerDay));
            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "Ticks per hour", System.TimeSpan.TicksPerHour));
            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "Ticks per minute", System.TimeSpan.TicksPerMinute));
            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "Ticks per second", System.TimeSpan.TicksPerSecond));
            ticksInfoBuilder.AppendLine(System.String.Format(numberFmt, "Ticks per millisecond", System.TimeSpan.TicksPerMillisecond));
            ticksInfoBuilder.AppendLine("--------------------------------------------------------------");
            return ticksInfoBuilder.ToString();
        }
    }
}
