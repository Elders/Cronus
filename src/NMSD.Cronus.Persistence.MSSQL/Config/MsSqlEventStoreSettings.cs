using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.EventSourcing.Config;

namespace NMSD.Cronus.Persitence.MSSQL.Config
{
    public class MsSqlEventStoreSettings : EventStoreSettings
    {
        public string ConnectionString { get; set; }

        public override IEventStore Build()
        {
            var storageManager = new MsSqlEventStoreStorageManager(BoundedContext, ConnectionString);
            storageManager.CreateStorage();
            return new MssqlEventStore(BoundedContext, ConnectionString, GlobalSettings.serializer);
        }
    }
}
