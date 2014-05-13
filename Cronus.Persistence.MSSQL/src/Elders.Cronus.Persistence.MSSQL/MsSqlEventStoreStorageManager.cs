using System;
using System.Data.SqlClient;
using System.Threading;
using Elders.Cronus.EventSourcing;

namespace Elders.Cronus.Persistence.MSSQL
{
    public class MsSqlEventStoreStorageManager : IEventStoreStorageManager
    {
        const string CreateEventsTableQuery = @"USE [{0}] SET ANSI_NULLS ON SET QUOTED_IDENTIFIER ON SET ANSI_PADDING ON CREATE TABLE [dbo].[{1}]([Revision] [int] IDENTITY(1,1) NOT NULL,[Events] [varbinary](max) NOT NULL,[EventsCount] [smallint] NOT NULL,[Timestamp] [datetime] NOT NULL,CONSTRAINT [PK_{1}BoundedContext] PRIMARY KEY CLUSTERED ([Revision] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] SET ANSI_PADDING OFF";

        const string CreateSnapshotsTableQuery = @"USE [{0}]  SET ANSI_NULLS ON  SET QUOTED_IDENTIFIER ON  SET ANSI_PADDING ON  CREATE TABLE [dbo].[{1}]( [Version] [int] NOT NULL, [AggregateId] [uniqueidentifier] NOT NULL,[AggregateState] [varbinary](max) NOT NULL, [Timestamp] [datetime] NOT NULL, CONSTRAINT [PK_{1}BoundedContextSnapshots] PRIMARY KEY CLUSTERED ([Version] ASC, [AggregateId] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]  SET ANSI_PADDING OFF ";

        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MsSqlEventStoreStorageManager));

        private readonly string connectionString;

        private static object locker = new object();

        private readonly IMsSqlEventStoreTableNameStrategy tableNameStrategy;

        public MsSqlEventStoreStorageManager(IMsSqlEventStoreTableNameStrategy tableNameStrategy, string connectionString)
        {
            this.tableNameStrategy = tableNameStrategy;
            this.connectionString = connectionString;
        }

        public void CreateEventsStorage()
        {
            CreateTable(CreateEventsTableQuery, tableNameStrategy.GetEventsTableName());
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
                    if (!DatabaseManager.DatabaseExists(connectionString))
                    {
                        DatabaseManager.CreateDatabase(connectionString, enableSnapshotIsolation: true);

                        while (!DatabaseManager.DatabaseExists(connectionString))
                            Thread.Sleep(50);
                    }

                    if (!DatabaseManager.TableExists(connectionString, tableNameStrategy.GetEventsTableName()))
                    {
                        CreateEventsStorage();
                        while (!DatabaseManager.TableExists(connectionString, tableNameStrategy.GetEventsTableName()))
                            Thread.Sleep(50);
                    }
                    if (!DatabaseManager.TableExists(connectionString, tableNameStrategy.GetSnapshotsTableName()))
                    {
                        CreateSnapshotsStorage();
                        while (!DatabaseManager.TableExists(connectionString, tableNameStrategy.GetSnapshotsTableName()))
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
            CreateTable(CreateSnapshotsTableQuery, tableNameStrategy.GetSnapshotsTableName());
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
