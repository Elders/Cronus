using System.Collections.Generic;

namespace Elders.Cronus.MessageProcessing
{
    public interface IFeedResult
    {
        ISet<TransportMessage> SuccessfulMessages { get; }
        ISet<TransportMessage> FailedMessages { get; }
    }
}