using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.AutoUpdater
{
    [DataContract(Name = "5ace83c0-c3e9-4192-9f5b-2485a02c49dc")]
    public class AutoUpdaterId : AggregateRootId
    {
        private const string ArName = "autoupdater";

        AutoUpdaterId() { }

        public AutoUpdaterId(string id, string tenant) : base(tenant, ArName, id) { }
    }
}
