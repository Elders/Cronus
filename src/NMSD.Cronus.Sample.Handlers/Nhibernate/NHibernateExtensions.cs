using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;

namespace NMSD.Cronus.Sample.Nhibernate.UoW
{
    public static class NHibernateExtensions
    {
        /// <summary>
        /// Adds all Hyperion mappings to a NHibernate configuration.
        /// </summary>
        /// <param name="nhConf">The NHib configuration instance.</param>
        /// <returns>Returns the NHib configuration instance with Hyperion mappings.</returns>
        public static Configuration AddAutoMappings(this Configuration nhConf, IEnumerable<Type> types, Action<ConventionModelMapper> modelMapper = null)
        {
            var englishPluralizationService = PluralizationService.CreateService(new CultureInfo("en-US"));
            var mapper = new ConventionModelMapper();

            //var baseEntityType = typeof(IEventHandler);
            //mapper.IsEntity((t, declared) => baseEntityType.IsAssignableFrom(t) && baseEntityType != t && !t.IsInterface);
            //mapper.IsRootEntity((t, declared) => baseEntityType.Equals(t.BaseType));

            mapper.BeforeMapClass += (insp, prop, map) => map.Table(englishPluralizationService.Pluralize(prop.Name));

            mapper.BeforeMapClass += (i, t, cm) => cm.Id(map =>
            {
                map.Column(t.Name + "Id");

                if (t.GetProperty("Id").PropertyType == typeof(Int32))
                    map.Generator(Generators.Identity);
                else
                    map.Generator(Generators.Assigned);
            });

            mapper.BeforeMapManyToOne += (insp, prop, map) => map.Column(prop.LocalMember.GetPropertyOrFieldType().Name + "Id");
            mapper.BeforeMapManyToOne += (insp, prop, map) => map.Cascade(Cascade.None);

            mapper.BeforeMapBag += (insp, prop, map) => map.Key(km => km.Column(prop.GetContainerEntity(insp).Name + "Id"));
            mapper.BeforeMapBag += (insp, prop, map) =>
            {
                map.Cascade(Cascade.All.Include(Cascade.DeleteOrphans));
                map.Inverse(true);
            };


            mapper.BeforeMapSet += (insp, prop, map) => map.Key(km => km.Column(prop.GetContainerEntity(insp).Name + "Id"));
            mapper.BeforeMapSet += (insp, prop, map) =>
            {
                map.Cascade(Cascade.All.Include(Cascade.DeleteOrphans));
                map.Inverse(true);
            };

            if (modelMapper != null)
                modelMapper(mapper);

            var mapping = mapper.CompileMappingFor(types);

            nhConf.AddDeserializedMapping(mapping, "Hyperion");

            return nhConf;
        }



        /// <summary>
        /// Drops the database based on the mappings.
        /// </summary>
        /// <param name="nhConf">The NHib configuration instance.</param>
        /// <returns>Returns the NHib configuration instance.</returns>
        public static Configuration DropDatabaseTables(this Configuration nhConf)
        {
            new SchemaExport(nhConf).Drop(false, true);
            return nhConf;
        }

        /// <summary>
        /// Drops and creates the database based on the mappings.
        /// </summary>
        /// <param name="nhConf">The NHib configuration instance.</param>
        /// <returns>Returns the NHib configuration instance.</returns>
        public static Configuration CreateDatabaseTables(this Configuration nhConf)
        {
            new SchemaExport(nhConf).Create(false, true);
            return nhConf;
        }
    }
    public static class DatabaseManager
    {
        private const string CreateDatabaseQuery = "USE master CREATE DATABASE {0} ON (NAME = {0}, FILENAME ='{1}{0}.mdf') LOG ON (NAME = {0}_log, FILENAME ='{1}{0}.ldf') COLLATE SQL_Latin1_General_CP1_CI_AS";
        private const string IsDatabaseExistsQuery = "USE master SELECT name FROM master.dbo.sysdatabases WHERE name = N'{0}'";
        private const string DeleteDatabaseQuery = "USE master ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE DROP DATABASE [{0}]";
        private const string DeleteDatabaseTables = @"use {0} begin tran DECLARE @TableName NVARCHAR(MAX) DECLARE @ConstraintName NVARCHAR(MAX) DECLARE DisableConstraints CURSOR FOR SELECT name as TABLE_NAME FROM sys.tables a WHERE name != 'Commits' and name != 'Snapshots'  OPEN DisableConstraints  FETCH NEXT FROM DisableConstraints INTO @TableName WHILE @@FETCH_STATUS = 0 BEGIN   EXEC('ALTER TABLE [' + @TableName + '] NOCHECK CONSTRAINT all')  FETCH NEXT FROM DisableConstraints INTO @TableName END print 'Done Disable Constraints' CLOSE DisableConstraints DEALLOCATE DisableConstraints DECLARE Constraints CURSOR FOR SELECT  TABLE_NAME, CONSTRAINT_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE TABLE_NAME != 'Commits' and TABLE_NAME != 'Snapshots' OPEN Constraints FETCH NEXT FROM Constraints INTO @TableName, @ConstraintName WHILE @@FETCH_STATUS = 0 BEGIN EXEC('ALTER TABLE [' + @TableName + '] NOCHECK CONSTRAINT all') 		 if exists (SELECT	CONSTRAINT_NAME 	FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 	WHERE CONSTRAINT_NAME = @ConstraintName 	and TABLE_NAME = @TableName)	BEGIN 		EXEC('ALTER TABLE [' + @TableName + '] DROP CONSTRAINT [' + @ConstraintName + ']')	END  FETCH NEXT FROM Constraints INTO @TableName, @ConstraintName END  CLOSE Constraints DEALLOCATE Constraints  print 'Done Constraints' DECLARE Tables CURSOR FOR  SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE (TABLE_NAME != 'Snapshots' and TABLE_NAME != 'Commits')  OPEN Tables FETCH NEXT FROM Tables INTO @TableName WHILE @@FETCH_STATUS = 0 BEGIN EXEC('DROP TABLE [' + @TableName + ']') FETCH NEXT FROM Tables INTO @TableName END CLOSE Tables DEALLOCATE Tables print 'Done Tables' commit tran";
        private const string DefaultDataFilePathQuery = "SELECT SUBSTRING(physical_name, 1, CHARINDEX(N'master.mdf', LOWER(physical_name)) - 1) DataFileLocation FROM master.sys.master_files WHERE database_id = 1 AND FILE_ID = 1";
        private const string ChangeSnapshotIsolation = @"USE master  ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE  ALTER DATABASE {0} SET ALLOW_SNAPSHOT_ISOLATION {1} ALTER DATABASE {0} SET READ_COMMITTED_SNAPSHOT {1}  ALTER DATABASE {0} SET MULTI_USER";

        public static bool CreateDatabase(string connectionString, string dataFilePath = "use_default", bool enableSnapshotIsolation = false)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            string dbName = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            if (Exists(connectionString))
                throw new Exception(String.Format("Database '{0}' exists.", dbName));


            SqlConnection conn = new SqlConnection(builder.ToString());

            try
            {
                conn.Open();

                if (dataFilePath == "use_default")
                    dataFilePath = GetDefaultFilePath(conn);

                var command = String.Format(CreateDatabaseQuery, dbName, dataFilePath);
                SqlCommand cmd = new SqlCommand(command, conn);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }

            bool canConnectToDatabse = false;
            for (int i = 0; i < 200; i++)
            {
                if (!DatabaseManager.TryConnect(connectionString))
                    Thread.Sleep(100);
                else
                    canConnectToDatabse = true;
            }
            if (canConnectToDatabse && enableSnapshotIsolation)
                EnableSnapshotIsolation(connectionString);

            return canConnectToDatabse;
        }

        private static string GetDefaultFilePath(SqlConnection conn)
        {
            SqlCommand pathCmd = new SqlCommand(DefaultDataFilePathQuery, conn);
            SqlDataReader dr = pathCmd.ExecuteReader();
            try
            {
                while (dr.Read())
                {
                    return dr["DataFileLocation"].ToString();
                }
                return "Cannot Find Default MSSQL database path.";
            }
            finally
            {
                dr.Close();
            }

        }

        public static void DeleteDatabase(string connectionString)
        {

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            string dbName = builder.InitialCatalog;
            builder.InitialCatalog = "master";
            if (!Exists(connectionString))
                return;


            SqlConnection conn = new SqlConnection(builder.ToString());

            try
            {
                conn.Open();
                var command = String.Format(DeleteDatabaseQuery, dbName);
                SqlCommand cmd = new SqlCommand(command, conn);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }

        public static void DropTablesWithoutEventStore(string connectionString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            string dbName = builder.InitialCatalog;
            builder.InitialCatalog = "master";
            if (!Exists(connectionString))
                return;

            SqlConnection conn = new SqlConnection(builder.ToString());

            try
            {
                conn.Open();
                var command = String.Format(DeleteDatabaseTables, dbName);

                SqlCommand cmd = new SqlCommand(command, conn);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }

        public static bool Exists(string connectionString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            string dbName = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            SqlConnection conn = new SqlConnection(builder.ToString());

            try
            {
                conn.Open();

                string query = String.Format(IsDatabaseExistsQuery, dbName);
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader dr = cmd.ExecuteReader();
                using (dr)
                {
                    return dr.HasRows;
                }
            }
            finally
            {
                conn.Close();
            }
        }

        static bool TryConnect(string connectionString)
        {
            bool isConnected = false;
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                conn.Open();
                isConnected = true;
            }
            catch (SqlException) { }
            finally
            {
                conn.Close();
            }
            return isConnected;
        }

        public static void EnableSnapshotIsolation(string connectionString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            SqlConnection conn = new SqlConnection(builder.ToString());

            try
            {
                conn.Open();
                string query = String.Format(ChangeSnapshotIsolation, builder.InitialCatalog, "ON");
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }

        public static void DisableSnapshotIsolation(string connectionString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            SqlConnection conn = new SqlConnection(builder.ToString());

            try
            {
                conn.Open();
                string query = String.Format(ChangeSnapshotIsolation, builder.InitialCatalog, "OFF");
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }
    }


}
