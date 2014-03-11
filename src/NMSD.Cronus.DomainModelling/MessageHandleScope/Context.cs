namespace NMSD.Cronus.Messaging.MessageHandleScope
{
    public class Context
    {
        public IScopeContext BatchScopeContext { get; set; }
        public IScopeContext MessageScopeContext { get; set; }
        public IScopeContext HandlerScopeContext { get; set; }
    }
}