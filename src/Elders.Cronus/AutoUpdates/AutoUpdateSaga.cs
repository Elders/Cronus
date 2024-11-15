using Elders.Cronus.EventStore.AutoUpdater;
using Elders.Cronus.EventStore.AutoUpdater.Commands;
using Elders.Cronus.EventStore.AutoUpdater.Events;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.AutoUpdates;

[DataContract(Name = "a460729e-d36f-4da6-beb2-a9d0180eb844")]
public class AutoUpdateSaga : Saga, ISystemSaga, // TODO: in future we can have scheduled messages
    IEventHandler<AutoUpdateTriggered>
{
    private readonly IAutoUpdaterStrategy strategy;

    public AutoUpdateSaga(IAutoUpdaterStrategy strategy, IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> timeoutRequestPublisher) : base(commandPublisher, timeoutRequestPublisher)
    {
        this.strategy = strategy;
    }

    public async Task HandleAsync(AutoUpdateTriggered @event)
    {
        IAutoUpdate updater = strategy.GetInstanceFor(@event.Name);
        bool finished = await updater.ApplyAsync();
        if (finished)
        {
            var id = new AutoUpdaterId(@event.BoundedContext, @event.Id.Tenant);

            var finish = new FinishAutoUpdate(id, @event.Name, DateTimeOffset.UtcNow);
            commandPublisher.Publish(finish);
        }
    }
}
