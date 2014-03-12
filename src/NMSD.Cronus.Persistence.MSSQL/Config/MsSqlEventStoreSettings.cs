using System.Configuration;
using System.Reflection;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.EventSourcing.Config;

namespace NMSD.Cronus.Persitence.MSSQL.Config
{
    public class MsSqlEventStoreSettings : EventStoreSettings
    {
        protected string connectionString;

        public override IEventStore Build()
        {
            if (createStorage)
                CreateStorageIfNotExists();

            GlobalSettings.Protoreg.RegisterAssembly(aggregateStatesAssembly);

            return new MssqlEventStore(boundedContext, connectionString, GlobalSettings.Serializer);
        }

        public MsSqlEventStoreSettings CreateStorage()
        {
            createStorage = true;

            return this;
        }

        public MsSqlEventStoreSettings SetAggregateStatesAssembly(Assembly aggregateStatesAssembly)
        {
            BuildAggregateStatesAssemblyAndBoundedContext(aggregateStatesAssembly);

            return this;
        }

        public MsSqlEventStoreSettings SetConnectionString(string connectionString)
        {
            this.connectionString = connectionString;

            return this;
        }

        public MsSqlEventStoreSettings SetConnectionStringName(string connectionStringName)
        {
            var connString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

            return SetConnectionString(connString);
        }

        private MsSqlEventStoreSettings CreateStorageIfNotExists()
        {
            var storageManager = new MsSqlEventStoreStorageManager(boundedContext, connectionString);
            storageManager.CreateStorage();

            return this;
        }

    }
}