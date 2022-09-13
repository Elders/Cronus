using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    public interface ICronusEventStoreIndex : IEventStoreIndex, ISystemHandler
    {
    }

    public interface IEventStoreIndex : IMessageHandler
    {
        Task IndexAsync(CronusMessage message);
    }

    public interface IEventStoreJobIndex : IMessageHandler
    {
        Task IndexAsync(AggregateCommit aggregateCommit);
    }

}
