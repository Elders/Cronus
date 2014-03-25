namespace Elders.Cronus.Messaging.MessageHandleScope
{
    public interface IScope
    {
        void Begin();

        void End();

        IScopeContext Context { get; set; }
    }
}