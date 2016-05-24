using System.Collections.Generic;
using Elders.Cronus.MessageProcessingMiddleware;
using Elders.Cronus.Middleware;

namespace Elders.Cronus
{
    public interface IMessageProcessor : IMiddleware<List<TransportMessage>, IFeedResult>
    {
        string Name { get; }
    }
}
