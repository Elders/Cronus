using System;

namespace NMSD.Cronus.Messaging.MessageHandleScope
{
    public class Context
    {
        public Guid Id { get; set; }

        public IScopeContext BatchScopeContext { get; set; }
        public IScopeContext MessageScopeContext { get; set; }
        public IScopeContext HandlerScopeContext { get; set; }
    }
}