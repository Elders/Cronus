using System;
using System.Collections.Generic;
using Elders.Cronus.Messaging.MessageHandleScope;

namespace Elders.Cronus
{
    public interface IMessageProcessor<TMessage>
    {
        ScopeFactory ScopeFactory { get; set; }
        int BatchSize { get; }
        SafeBatchResult<TMessage> Handle(List<TMessage> messages);
        IEnumerable<Type> GetRegisteredHandlers();
    }
}
