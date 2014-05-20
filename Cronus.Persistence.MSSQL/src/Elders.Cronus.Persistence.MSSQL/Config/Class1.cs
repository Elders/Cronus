using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.EventSourcing.Config;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Protoreg;

namespace Elders.Cronus.Persistence.MSSQL.Config
{
    public static class MsSqlEventStoreExtensions
    {
        public static T UseMsSqlEventStore<T>(this T self, Action<MsSqlEventStoreSettings> configure) where T : ICronusSettings, IHaveEventStores
        {
            MsSqlEventStoreSettings settings = new MsSqlEventStoreSettings();
            if (configure != null)
                configure(settings);

            self.CopySerializerTo(settings);

            self.EventStores.Add(settings.BoundedContext, settings.GetInstanceLazy());
            return self;
        }

        public static T SetConnectionStringName<T>(this T self, string connectionStringName) where T : MsSqlEventStoreSettings
        {
            self.ConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            return self;
        }

        public static T SetConnectionString<T>(this T self, string connectionString) where T : MsSqlEventStoreSettings
        {
            self.ConnectionString = connectionString;
            return self;
        }

        public static T SetAggregateStatesAssembly<T>(this T self, Type aggregateStatesAssembly) where T : MsSqlEventStoreSettings
        {
            return self.SetAggregateStatesAssembly(Assembly.GetAssembly(aggregateStatesAssembly));
        }

        public static T SetAggregateStatesAssembly<T>(this T self, Assembly aggregateStatesAssembly) where T : MsSqlEventStoreSettings
        {
            self.AggregateStatesAssembly = aggregateStatesAssembly;
            self.BoundedContext = aggregateStatesAssembly.GetAssemblyAttribute<BoundedContextAttribute>().BoundedContextName;
            self.EventStoreTableNameStrategy = new MsSqlEventStoreTableNameStrategy(self.BoundedContext);
            return self;
        }

        public static T WithNewStorageIfNotExists<T>(this T self) where T : MsSqlEventStoreSettings
        {
            var storageManager = new MsSqlEventStoreStorageManager(self.EventStoreTableNameStrategy, self.ConnectionString);
            storageManager.CreateStorage();
            return self;
        }
    }

    public class MsSqlEventStore : IEventStore
    {
        public MsSqlEventStore(IAggregateRepository aggregateRepository, IEventStorePersister persister, IEventStorePlayer player, IEventStoreStorageManager storageManager = null)
        {
            this.AggregateRepository = aggregateRepository;
            this.Persister = persister;
            this.Player = player;
            this.StorageManager = storageManager;
        }

        public IAggregateRepository AggregateRepository { get; private set; }

        public IEventStorePersister Persister { get; private set; }

        public IEventStorePlayer Player { get; private set; }

        public IEventStoreStorageManager StorageManager { get; private set; }
    }

    public interface IMsSqlEventStoreSettings : IEventStoreSettings
    {

    }

    public class MsSqlEventStoreSettings : IMsSqlEventStoreSettings
    {
        public Assembly AggregateStatesAssembly { get; set; }
        public string BoundedContext { get; set; }
        public Assembly DomainEventsAssembly { get; set; }
        public IMsSqlEventStoreTableNameStrategy EventStoreTableNameStrategy { get; set; }
        public ProtoregSerializer Serializer { get; set; }

        public string ConnectionString { get; set; }

        Lazy<IEventStore> ISettingsBuilder<IEventStore>.Build()
        {
            IEventStoreSettings settings = this as IEventStoreSettings;


            var persister = new MsSqlPersister(EventStoreTableNameStrategy, settings.Serializer, ConnectionString);
            var aggregateRepository = new MsSqlAggregateRepository(persister, EventStoreTableNameStrategy, settings.Serializer, ConnectionString);
            var player = new MsSqlEventStorePlayer(EventStoreTableNameStrategy, settings.Serializer, ConnectionString);
            var retryStrategy = new DefaultRetryStrategy<DomainMessageCommit>() as ISafeBatchRetryStrategy<DomainMessageCommit>;

            return new Lazy<IEventStore>(() => new MsSqlEventStore(aggregateRepository, persister, player, null));
        }
    }
}
