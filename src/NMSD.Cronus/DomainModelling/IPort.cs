using NMSD.Cronus.Messaging;

namespace NMSD.Cronus.DomainModelling
{
    public interface IPort
    {
        IPublisher CommandPublisher { get; set; }
    }
}