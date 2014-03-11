using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Persitence.MSSQL;
using NMSD.Protoreg;

namespace NMSD.Cronus.EventSourcing
{
    public class MssqlEventStore : IAggregateRepository, IEventStore
    {
        const string CreateEventsTableQuery = @"USE [{0}] SET ANSI_NULLS ON SET QUOTED_IDENTIFIER ON SET ANSI_PADDING ON CREATE TABLE [dbo].[{1}]([Revision] [int] IDENTITY(1,1) NOT NULL,[Events] [varbinary](max) NOT NULL,[EventsCount] [smallint] NOT NULL,[Timestamp] [datetime] NOT NULL,CONSTRAINT [PK_{1}BoundedContext] PRIMARY KEY CLUSTERED ([Revision] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] SET ANSI_PADDING OFF";

        const string CreateSnapshotsTableQuery = @"USE [{0}]  SET ANSI_NULLS ON  SET QUOTED_IDENTIFIER ON  SET ANSI_PADDING ON  CREATE TABLE [dbo].[{1}]( [Version] [int] NOT NULL, [AggregateId] [uniqueidentifier] NOT NULL,[AggregateState] [varbinary](max) NOT NULL, [Timestamp] [datetime] NOT NULL, CONSTRAINT [PK_{1}BoundedContextSnapshots] PRIMARY KEY CLUSTERED ([Version] ASC, [AggregateId] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]  SET ANSI_PADDING OFF ";

        const string DeleteAggregateStatesQueryTemplate = @"DELETE {0} WHERE [Timestamp]<@timestamp AND ({1})";

        const string DeleteAggregateStatesWhereTemplate = @"AggregateId=@aggregateId{0} AND [Version]<@version{0}";

        const string LoadAggregateStateQueryTemplate = @"SELECT TOP 1 AggregateState FROM {0} WHERE AggregateId=@aggregateId ORDER BY Version DESC";

        const string LoadEventsQueryTemplate = @"SELECT Events FROM {0} ORDER BY Revision OFFSET @offset ROWS FETCH NEXT {1} ROWS ONLY";

        const string TableExistsQuery = @"SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = '{0}'";

        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MssqlEventStore));

        private readonly string boundedContext;

        private readonly string connectionString;

        private readonly string eventsTableName;

        private static object locker = new object();

        private readonly ProtoregSerializer serializer;

        private readonly string snapshotsTableName;

        public MssqlEventStore(string boundedContext, string connectionString, ProtoregSerializer serializer)
        {
            this.boundedContext = boundedContext;
            eventsTableName = String.Format("dbo.{0}Events", boundedContext);
            snapshotsTableName = String.Format("dbo.{0}Snapshots", boundedContext);
            this.connectionString = connectionString;
            this.serializer = serializer;
        }

        public IEnumerable<IEvent> GetEventsFromStart(string boundedContext, int batchPerQuery = 1)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = String.Format(LoadEventsQueryTemplate, boundedContext + "Events", batchPerQuery);
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


        //public AR Update<AR>(IAggregateRootId aggregateId, Commanding.ICommand command, Action<AR> update, Action<IAggregateRoot> save = null) where AR : IAggregateRoot
        //{
        //    throw new NotImplementedException();
        //}
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

        public void Save(IAggregateRoot aggregateRoot, ICommand command)
        {
            //throw new NotImplementedException();
        }

        public void UseStream(Func<DomainMessageCommit> getCommit, Func<IEventStream, DomainMessageCommit, bool> commitCondition, Action<List<IEvent>> postCommit, Func<IEventStream, bool> closeStreamCondition, Action<DomainMessageCommit> onPersistError)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var newStream = new EventStream();
                List<DomainMessageCommit> commits = new List<DomainMessageCommit>();
                bool shouldCommit = false;

                while (!closeStreamCondition(newStream))
                {
                    while (!shouldCommit)
                    {
                        var commit = getCommit();

                        if (commit != default(DomainMessageCommit))
                        {
                            newStream.Events.AddRange(commit.Events);
                            newStream.Snapshots.Add(commit.State);
                            commits.Add(commit);
                        }

                        shouldCommit = commitCondition(newStream, commit);
                        if (shouldCommit)
                        {
                            if (newStream.Events.Count > 0)
                            {
                                try
                                {
                                    TakeSnapshot(newStream.Snapshots, conn);

                                    Persist(newStream.Events, conn);

                                    if (!ReferenceEquals(null, postCommit))
                                        postCommit(newStream.Events);
                                }
                                catch (AggregateStateFirstLevelConcurrencyException ex)
                                {
                                    foreach (var item in commits)
                                    {
                                        try
                                        {
                                            TakeSnapshot(new List<IAggregateRootState>() { item.State }, conn);
                                            Persist(new List<IEvent>(item.Events), conn);
                                            if (!ReferenceEquals(null, postCommit))
                                                postCommit(item.Events);

                                        }
                                        catch (AggregateStateFirstLevelConcurrencyException)
                                        {
                                            onPersistError(item);
                                        }
                                    }
                                }


                                newStream.Events.Clear();
                                newStream.Snapshots.Clear();
                                commits.Clear();

                            }
                            shouldCommit = false;   // reset
                        }
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

        private IAggregateRootState LoadAggregateState(Guid aggregateId)
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

        private void Persist(List<IEvent> events, SqlConnection connection)
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

        private SqlCommand PrepareAggregateStateDeleteCommand(List<IAggregateRootState> states)
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
                serializer.Serialize(stream, new EventBatchWraper(events.Cast<object>().ToList()));
                return stream.ToArray();
            }
        }

        private void TakeSnapshot(List<IAggregateRootState> states, SqlConnection connection)
        {
#if DEBUG
            ValidateSnapshotsBoundedContext(states);
#endif
            try
            {
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

                        //var statesDeleteCommand = PrepareAggregateStateDeleteCommand(states);
                        //statesDeleteCommand.Connection = connection;
                        //statesDeleteCommand.Transaction = tx;
                        //statesDeleteCommand.ExecuteNonQuery();

                        bulkCopy.BatchSize = 100;
                        bulkCopy.WriteToServer(dt, DataRowState.Added);
                        // tx.Commit();

                    }

                }
            }
            catch (Exception ex)
            {
                // tx.Rollback();
                throw new AggregateStateFirstLevelConcurrencyException("", ex);
            }

        }

        private void ValidateEventsBoundedContext(IList<IEvent> events)
        {
            StringBuilder errors = new StringBuilder();
            ISet<Type> wrongEventTypes = new HashSet<Type>();
            foreach (var @event in events)
            {
                var eventType = @event.GetType();
                string eventBC = eventType.GetBoundedContext().BoundedContextName;
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
                string eventBC = eventType.GetBoundedContext().BoundedContextName;
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


    }


    public interface IEventStreamProcessor
    {
        DomainMessageCommit GetCommit();
        bool CommitCondition(IEventStream eventStream, DomainMessageCommit commit);
        void PostCommit(IEventStream eventStream);
        bool CloseStreamCondition(IEventStream eventStream);
    }
}
