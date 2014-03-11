using System;
using NMSD.Cronus.EventSourcing.Config;

namespace NMSD.Cronus.Persitence.MSSQL.Config
{
    public static class MsSqlEventStoreConfig
    {
        public static EventStoreSettings MsSql(this EventStoreSettings common, Action<MsSqlEventStoreSettings> configure)
        {
            var cfg = new MsSqlEventStoreSettings(common);
            common.Builder = cfg;
            configure(cfg);
            return common;
        }
    }
}
