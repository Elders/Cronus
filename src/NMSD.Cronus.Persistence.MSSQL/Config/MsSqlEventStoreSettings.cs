using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.EventSourcing.Config;

namespace NMSD.Cronus.Persitence.MSSQL.Config
{
    public class MsSqlEventStoreSettings : IEventStoreBuilder
    {
        private readonly EventStoreSettings common;

        public MsSqlEventStoreSettings(EventStoreSettings common)
        {
            this.common = common;
        }
        public string ConnectionString { get; set; }

        public IEventStore Build()
        {
            var storageManager = new MsSqlEventStoreStorageManager(common.BoundedContext, ConnectionString);
            storageManager.CreateStorage();
            return new MssqlEventStore(common.BoundedContext, ConnectionString, common.GlobalSettings.serializer);
        }
    }
}
