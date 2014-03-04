namespace NMSD.Cronus.Messaging.MessageHandleScope
{
    public interface IScopeContext
    {
        void Set<T>(T obj);
        T Get<T>();
        void Clear();
    }
}