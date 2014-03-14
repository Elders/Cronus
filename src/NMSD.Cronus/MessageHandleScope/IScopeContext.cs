namespace NMSD.Cronus.Messaging.MessageHandleScope
{
    public interface IScopeContext
    {
        void Set<T>(T obj);
        void Set<T>(string name, T obj);
        T Get<T>();
        T Get<T>(string name);
        void Clear();
    }
}