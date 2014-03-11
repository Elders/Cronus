using System;
using System.Data.SqlClient;
using System.Threading;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.EventSourcing;

namespace NMSD.Cronus.Persitence.MSSQL
{
    public class MsSqlEventStoreStorageManager : IEventStoreStorageManager
    {
        const string CreateEventsTableQuery = @"USE [{0}] SET ANSI_NULLS ON SET QUOTED_IDENTIFIER ON SET ANSI_PADDING ON CREATE TABLE [dbo].[{1}]([Revision] [int] IDENTITY(1,1) NOT NULL,[Events] [varbinary](max) NOT NULL,[EventsCount] [smallint] NOT NULL,[Timestamp] [datetime] NOT NULL,CONSTRAINT [PK_{1}BoundedContext] PRIMARY KEY CLUSTERED ([Revision] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] SET ANSI_PADDING OFF";

        const string CreateSnapshotsTableQuery = @"USE [{0}]  SET ANSI_NULLS ON  SET QUOTED_IDENTIFIER ON  SET ANSI_PADDING ON  CREATE TABLE [dbo].[{1}]( [Version] [int] NOT NULL, [AggregateId] [uniqueidentifier] NOT NULL,[AggregateState] [varbinary](max) NOT NULL, [Timestamp] [datetime] NOT NULL, CONSTRAINT [PK_{1}BoundedContextSnapshots] PRIMARY KEY CLUSTERED ([Version] ASC, [AggregateId] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]  SET ANSI_PADDING OFF ";

        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MsSqlEventStoreStorageManager));

        private readonly string boundedContext;

        private readonly string connectionString;

        private readonly string eventsTableName;

        private static object locker = new object();

        private readonly string snapshotsTableName;

        public MsSqlEventStoreStorageManager(string boundedContext, string connectionString)
        {
            this.boundedContext = boundedContext;
            this.connectionString = connectionString;
            eventsTableName = String.Format("dbo.{0}Events", boundedContext);
            snapshotsTableName = String.Format("dbo.{0}Snapshots", boundedContext);
        }

        public void CreateEventsStorage()
        {
            CreateTable(CreateEventsTableQuery, eventsTableName);
        }

        /// <summary>
        /// Creates the database for the event store including the Events and Snapshots tables. If any exists it is not overwritten.
        /// </summary>
        public void CreateStorage()
        {
            try
            {
                lock (locker)
                {
                    if (!DatabaseManager.Exists(connectionString))
                    {
                        DatabaseManager.CreateDatabase(connectionString, enableSnapshotIsolation: true);

                        while (!DatabaseManager.Exists(connectionString))
                            Thread.Sleep(50);
                    }

                    if (!DatabaseManager.TableExists(connectionString, eventsTableName))
                    {
                        CreateEventsStorage();
                        while (!DatabaseManager.TableExists(connectionString, eventsTableName))
                            Thread.Sleep(50);
                    }
                    if (!DatabaseManager.TableExists(connectionString, snapshotsTableName))
                    {
                        CreateSnapshotsStorage();
                        while (!DatabaseManager.TableExists(connectionString, snapshotsTableName))
                            Thread.Sleep(50);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Could not create EventStore.", ex);
            }
        }

        public void CreateSnapshotsStorage()
        {
            CreateTable(CreateSnapshotsTableQuery, snapshotsTableName);
        }

        private void CreateTable(string queryTemplate, string tableName)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);

            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                conn.Open();

                var command = String.Format(queryTemplate, builder.InitialCatalog, tableName.Replace("dbo.", ""));
                SqlCommand cmd = new SqlCommand(command, conn);
                cmd.ExecuteNonQuery();

            }
            finally
            {
                conn.Close();
            }
        }

    }
}
