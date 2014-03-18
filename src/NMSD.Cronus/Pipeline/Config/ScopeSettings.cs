using NMSD.Cronus.Messaging.MessageHandleScope;

namespace NMSD.Cronus.Pipeline.Config
{
    public class ScopeSettings
    {
        public IBatchScope BatchScope { get; set; }
        public IMessageScope MessageScope { get; set; }
        public IHandlerScope HandlerScope { get; set; }
    }
}