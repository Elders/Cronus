using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Cassandra;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.EventSourcing.Config;

namespace Elders.Cronus.Persistence.Cassandra.Config
{
    public class CassandraEventStoreSettings : EventStoreSettings
    {
        protected string keyspace;

        protected Session session;

        protected ICassandraEventStoreTableNameStrategy tableNameStrategy;

        public override IAggregateRepository BuildAggregateRepository()
        {
            GlobalSettings.Protoreg.RegisterAssembly(aggregateStatesAssembly);
            return new CassandraAggregateRepository(session, GlobalSettings.EventPublisher, GlobalSettings.EventStorePersisters[BoundedContext], tableNameStrategy, GlobalSettings.Serializer);
        }

        public override IMessageProcessor<DomainMessageCommit> BuildEventStoreHandlers()
        {
            return null;
        }

        public override IEventStorePersister BuildEventStorePersister()
        {
            if (createStorage)
                CreateStorageIfNotExists();

            GlobalSettings.Protoreg.RegisterAssembly(aggregateStatesAssembly);
            return new CassandraPersister(session, tableNameStrategy, GlobalSettings.Serializer);
        }

        public override IEventStorePlayer BuildEventStorePlayer()
        {
            GlobalSettings.Protoreg.RegisterAssembly(aggregateStatesAssembly);
            return new CassandraEventStorePlayer();
        }

        public CassandraEventStoreSettings CreateStorage()
        {
            createStorage = true;

            return this;
        }

        public CassandraEventStoreSettings SetAggregateStatesAssembly(Type aggregateStatesAssembly)
        {
            return SetAggregateStatesAssembly(Assembly.GetAssembly(aggregateStatesAssembly));
        }

        public CassandraEventStoreSettings SetAggregateStatesAssembly(Assembly aggregateStatesAssembly)
        {
            BuildAggregateStatesAssemblyAndBoundedContext(aggregateStatesAssembly);
            tableNameStrategy = new CassandraEventStoreTableNameStrategy(BoundedContext);
            return this;
        }

        public CassandraEventStoreSettings SetConnectionString(string connectionString)
        {
            var cluster = Cluster
                .Builder()
                .WithConnectionString(connectionString)
                .Build();
            session = cluster.ConnectAndCreateDefaultKeyspaceIfNotExists();
            keyspace = session.Keyspace;

            return this;
        }

        public CassandraEventStoreSettings SetConnectionStringName(string connectionStringName)
        {
            var connectionStringElement = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionStringElement.ProviderName != this.GetType().Assembly.GetName().Name)
                throw new ConfigurationErrorsException(String.Format("Invalid cassandra connection string provider. Expected provider name is '{0}' but it was '{1}'.", this.GetType().Assembly.GetName().Name, connectionStringElement.ProviderName));

            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            return SetConnectionString(connectionString);
        }

        public CassandraEventStoreSettings SetDomainEventsAssembly(Assembly domainEventsAssembly)
        {
            this.domainEventsAssembly = domainEventsAssembly;
            return this;
        }

        public CassandraEventStoreSettings SetDomainEventsAssembly(Type domainEventsAssembly)
        {
            return SetDomainEventsAssembly(Assembly.GetAssembly(domainEventsAssembly));
        }

        private CassandraEventStoreSettings CreateStorageIfNotExists()
        {
            var storageManager = new CassandraEventStoreStorageManager(session, tableNameStrategy);
            storageManager.CreateStorage();

            return this;
        }

    }
}