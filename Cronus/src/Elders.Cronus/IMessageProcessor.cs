using System;
using System.Collections.Generic;

namespace Elders.Cronus
{
    public interface IMessageProcessor<TMessage>
    {
        int BatchSize { get; }
        SafeBatchResult<TMessage> Handle(List<TMessage> messages);
        IEnumerable<Type> GetRegisteredHandlers();
    }
}
