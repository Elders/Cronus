using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.EventSourcing.Config;

namespace Elders.Cronus.Persistence.MSSQL.Config
{
    public class MsSqlEventStoreSettings : EventStoreSettings
    {
        protected string connectionString;

        protected IMsSqlEventStoreTableNameStrategy tableNameStrategy;

        public override IAggregateRepository BuildAggregateRepository()
        {
            GlobalSettings.Protoreg.RegisterAssembly(aggregateStatesAssembly);
            return new MsSqlAggregateRepository(GlobalSettings.EventStorePersisters[BoundedContext], tableNameStrategy, GlobalSettings.Serializer, connectionString);
        }

        public override IMessageProcessor<DomainMessageCommit> BuildEventStoreHandlers()
        {
            var persister = GlobalSettings.EventStorePersisters[BoundedContext];
            var retryStrategy = new DefaultRetryStrategy<DomainMessageCommit>() as ISafeBatchRetryStrategy<DomainMessageCommit>;
            var handler = new EventStoreHandler(domainEventsAssembly.ExportedTypes.First(), new EventStoreSafeBatchContextFactory(retryStrategy, persister), 100);
            return handler;
        }

        public override IEventStorePersister BuildEventStorePersister()
        {
            if (createStorage)
                CreateStorageIfNotExists();

            GlobalSettings.Protoreg.RegisterAssembly(aggregateStatesAssembly);
            return new MsSqlPersister(tableNameStrategy, GlobalSettings.Serializer, connectionString);
        }

        public override IEventStorePlayer BuildEventStorePlayer()
        {
            GlobalSettings.Protoreg.RegisterAssembly(aggregateStatesAssembly);
            return new MsSqlEventStorePlayer(tableNameStrategy, GlobalSettings.Serializer, connectionString);
        }

        public MsSqlEventStoreSettings CreateStorage()
        {
            createStorage = true;

            return this;
        }

        public MsSqlEventStoreSettings SetAggregateStatesAssembly(Type aggregateStatesAssembly)
        {
            return SetAggregateStatesAssembly(Assembly.GetAssembly(aggregateStatesAssembly));
        }

        public MsSqlEventStoreSettings SetAggregateStatesAssembly(Assembly aggregateStatesAssembly)
        {
            BuildAggregateStatesAssemblyAndBoundedContext(aggregateStatesAssembly);
            tableNameStrategy = new MsSqlEventStoreTableNameStrategy(BoundedContext);
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

        public MsSqlEventStoreSettings SetDomainEventsAssembly(Assembly domainEventsAssembly)
        {
            this.domainEventsAssembly = domainEventsAssembly;
            return this;
        }

        public MsSqlEventStoreSettings SetDomainEventsAssembly(Type domainEventsAssembly)
        {
            return SetDomainEventsAssembly(Assembly.GetAssembly(domainEventsAssembly));
        }

        private MsSqlEventStoreSettings CreateStorageIfNotExists()
        {
            var storageManager = new MsSqlEventStoreStorageManager(tableNameStrategy, connectionString);
            storageManager.CreateStorage();

            return this;
        }

    }
}