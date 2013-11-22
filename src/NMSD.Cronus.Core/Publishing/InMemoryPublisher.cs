using System;
using System.Collections.Generic;

namespace NMSD.Cronus.Core.Publishing
{
    public abstract class InMemoryPublisher<TMessage, THandler> : Publisher<TMessage, THandler>
    {
        public override bool Publish(TMessage message)
        {
            List<Func<TMessage, bool>> availableHandlers;
            if (handlers.TryGetValue(message.GetType(), out availableHandlers))
            {
                foreach (var handleMethod in availableHandlers)
                {
                    var result = handleMethod(message);
                    if (result == false)
                        return result;
                }
            }
            return true;
        }
    }
}