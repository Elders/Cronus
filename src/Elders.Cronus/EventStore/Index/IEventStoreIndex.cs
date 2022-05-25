using System.Threading.Tasks;

namespace Elders.Cronus.EventStore.Index
{
    public interface ICronusEventStoreIndex : IEventStoreIndex, ISystemHandler, IMessageHandler
    {
    }

    public interface IEventStoreIndex : IMessageHandler
    {
        Task IndexAsync(CronusMessage message);
    }
}
