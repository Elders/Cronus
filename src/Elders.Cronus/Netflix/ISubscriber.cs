using System.Collections.Generic;

namespace Elders.Cronus.Netflix
{
    public interface ISubscriber
    {
        string Id { get; }

        List<string> MessageTypes { get; }

        void Process(CronusMessage message);
    }
}