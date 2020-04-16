namespace Elders.Cronus.EventStore.Index
{
    public interface IIndexStatusStore
    {
        void Save(string indexId, IndexStatus status);
        IndexStatus Get(string indexId);
    }
}
