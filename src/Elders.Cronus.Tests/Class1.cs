using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Tests
{
    public class NulllEventPublisher : IPublisher<IEvent>
    {
        public bool Publish(IEvent message)
        {
            return true;
        }
    }
}
