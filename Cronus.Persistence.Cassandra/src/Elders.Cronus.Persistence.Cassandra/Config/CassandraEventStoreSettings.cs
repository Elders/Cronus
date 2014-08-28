using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Cassandra;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.EventSourcing.Config;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Persistence.Cassandra.Config
{
    public static class CassandraEventStoreExtensions
    {
        public static T UseCassandraEventStore<T>(this T self, Action<CassandraEventStoreSettings> configure) where T : ICronusSettings, IHaveEventStores
        {
            CassandraEventStoreSettings settings = new CassandraEventStoreSettings();
            if (configure != null)
                configure(settings);

            self.CopySerializerTo(settings);

            self.EventStores.Add((settings as ICassandraEventStoreSettings).BoundedContext, settings.GetInstanceLazy());
            return self;
        }

        public static T SetConnectionStringName<T>(this T self, string connectionStringName) where T : ICassandraEventStoreSettings
        {
            return self.SetConnectionString(ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString);
        }

        public static T SetConnectionString<T>(this T self, string connectionString) where T : ICassandraEventStoreSettings
        {
            var cluster = Cluster
                .Builder()
                .WithConnectionString(connectionString)
                .Build();
            self.Session = cluster.ConnectAndCreateDefaultKeyspaceIfNotExists();
            self.KeySpace = self.Session.Keyspace;

            return self;
        }

        public static T SetAggregateStatesAssembly<T>(this T self, Type aggregateStatesAssembly) where T : ICassandraEventStoreSettings
        {
            return self.SetAggregateStatesAssembly(Assembly.GetAssembly(aggregateStatesAssembly));
        }

        public static T SetAggregateStatesAssembly<T>(this T self, Assembly aggregateStatesAssembly) where T : ICassandraEventStoreSettings
        {
            self.BoundedContext = aggregateStatesAssembly.GetAssemblyAttribute<BoundedContextAttribute>().BoundedContextName;
            self.EventStoreTableNameStrategy = new CassandraEventStoreTableNameStrategy(self.BoundedContext);
            return self;
        }

        public static T WithNewStorageIfNotExists<T>(this T self) where T : ICassandraEventStoreSettings
        {
            var storageManager = new CassandraEventStoreStorageManager(self.Session, self.EventStoreTableNameStrategy);
            storageManager.CreateStorage();
            return self;
        }
    }

    public interface ICassandraEventStoreSettings : IEventStoreSettings
    {
        string ConnectionString { get; set; }
        string KeySpace { get; set; }
        ISession Session { get; set; }
        ICassandraEventStoreTableNameStrategy EventStoreTableNameStrategy { get; set; }
    }

    public class CassandraEventStoreSettings : ICassandraEventStoreSettings
    {
        string IEventStoreSettings.BoundedContext { get; set; }

        string ICassandraEventStoreSettings.ConnectionString { get; set; }

        ICassandraEventStoreTableNameStrategy ICassandraEventStoreSettings.EventStoreTableNameStrategy { get; set; }

        string ICassandraEventStoreSettings.KeySpace { get; set; }

        ISerializer IHaveSerializer.Serializer { get; set; }

        ISession ICassandraEventStoreSettings.Session { get; set; }

        Lazy<IEventStore> ISettingsBuilder<IEventStore>.Build()
        {
            ICassandraEventStoreSettings settings = this as ICassandraEventStoreSettings;

            var persister = new CassandraPersister(settings.Session, settings.EventStoreTableNameStrategy, settings.Serializer);
            var aggregateRepository = new CassandraAggregateRepository(settings.Session, persister, settings.EventStoreTableNameStrategy, settings.Serializer);
            var player = new CassandraEventStorePlayer();

            return new Lazy<IEventStore>(() => new CassandraEventStore(aggregateRepository, persister, player, null));
        }
    }
}