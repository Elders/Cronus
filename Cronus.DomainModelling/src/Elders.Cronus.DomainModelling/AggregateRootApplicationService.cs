namespace Elders.Cronus.DomainModelling
{
    public interface IAggregateRootApplicationService
    {
        IAggregateRepository Repository { get; set; }
    }

    public class AggregateRootApplicationService<AR> : IAggregateRootApplicationService where AR : IAggregateRoot
    {
        public IAggregateRepository Repository { get; set; }
    }
}