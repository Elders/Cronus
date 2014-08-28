using System;
using System.Configuration;
using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.EventSourcing.Config;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Serializer;

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

            self.EventStores.Add((settings as IMsSqlEventStoreSettings).BoundedContext, settings.GetInstanceLazy());
            return self;
        }

        public static T SetConnectionStringName<T>(this T self, string connectionStringName) where T : IMsSqlEventStoreSettings
        {
            self.ConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            return self;
        }

        public static T SetConnectionString<T>(this T self, string connectionString) where T : IMsSqlEventStoreSettings
        {
            self.ConnectionString = connectionString;
            return self;
        }

        public static T SetAggregateStatesAssembly<T>(this T self, Type aggregateStatesAssembly) where T : IMsSqlEventStoreSettings
        {
            return self.SetAggregateStatesAssembly(Assembly.GetAssembly(aggregateStatesAssembly));
        }

        public static T SetAggregateStatesAssembly<T>(this T self, Assembly aggregateStatesAssembly) where T : IMsSqlEventStoreSettings
        {
            self.BoundedContext = aggregateStatesAssembly.GetAssemblyAttribute<BoundedContextAttribute>().BoundedContextName;
            self.EventStoreTableNameStrategy = new MsSqlEventStoreTableNameStrategy(self.BoundedContext);
            return self;
        }

        public static T WithNewStorageIfNotExists<T>(this T self) where T : IMsSqlEventStoreSettings
        {
            var storageManager = new MsSqlEventStoreStorageManager(self.EventStoreTableNameStrategy, self.ConnectionString);
            storageManager.CreateStorage();
            return self;
        }
    }

    public interface IMsSqlEventStoreSettings : IEventStoreSettings
    {
        string ConnectionString { get; set; }
        IMsSqlEventStoreTableNameStrategy EventStoreTableNameStrategy { get; set; }
    }

    public class MsSqlEventStoreSettings : IMsSqlEventStoreSettings
    {
        string IEventStoreSettings.BoundedContext { get; set; }

        string IMsSqlEventStoreSettings.ConnectionString { get; set; }

        IMsSqlEventStoreTableNameStrategy IMsSqlEventStoreSettings.EventStoreTableNameStrategy { get; set; }

        ISerializer IHaveSerializer.Serializer { get; set; }

        Lazy<IEventStore> ISettingsBuilder<IEventStore>.Build()
        {
            IMsSqlEventStoreSettings settings = this as IMsSqlEventStoreSettings;

            var persister = new MsSqlPersister(settings.EventStoreTableNameStrategy, settings.Serializer, settings.ConnectionString);
            var aggregateRepository = new MsSqlAggregateRepository(persister, settings.EventStoreTableNameStrategy, settings.Serializer, settings.ConnectionString);
            var player = new MsSqlEventStorePlayer(settings.EventStoreTableNameStrategy, settings.Serializer, settings.ConnectionString);

            return new Lazy<IEventStore>(() => new MsSqlEventStore(aggregateRepository, persister, player, null));
        }
    }
}
