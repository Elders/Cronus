using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "b11705a2-6744-4ca6-8480-e887c3fc09f2")]
    public class EventStoreIndexManagerId : StringTenantId
    {
        EventStoreIndexManagerId() : base() { }

        public EventStoreIndexManagerId(string indexName, string tenant) : base(indexName, "eventstoreindexmanager", tenant) { }
        public EventStoreIndexManagerId(StringTenantUrn urn) : base(urn, "eventstoreindexmanager") { }
    }
}
