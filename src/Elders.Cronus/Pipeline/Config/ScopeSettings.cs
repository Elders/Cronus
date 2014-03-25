using Elders.Cronus.Messaging.MessageHandleScope;

namespace Elders.Cronus.Pipeline.Config
{
    public class ScopeSettings
    {
        public IBatchScope BatchScope { get; set; }
        public IMessageScope MessageScope { get; set; }
        public IHandlerScope HandlerScope { get; set; }
    }
}