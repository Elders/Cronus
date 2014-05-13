using System.Configuration;

namespace Elders.Cronus.Persistence.Cassandra.Config
{
    public class CassandraConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("connectionString")]
        public string ConnectionString
        {
            get { return this["connectionString"].ToString(); }
        }
    }
}