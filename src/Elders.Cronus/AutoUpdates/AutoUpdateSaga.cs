using Elders.Cronus.EventStore.AutoUpdater;
using Elders.Cronus.EventStore.AutoUpdater.Commands;
using Elders.Cronus.EventStore.AutoUpdater.Events;
using System;
using System.Runtime.Serialization;
using System.Threading;
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

    /// <summary>
    /// Applies the auto-update strategy that matches <see cref="AutoUpdateTriggered.Name"/> and finishes the auto-updater on completion.
    /// </summary>
    /// <param name="event">The event signalling that an auto-update has been triggered.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task HandleAsync(AutoUpdateTriggered @event, CancellationToken cancellationToken = default)
    {
        IAutoUpdate updater = strategy.GetInstanceFor(@event.Name);
        bool finished = await updater.ApplyAsync().ConfigureAwait(false);
        if (finished)
        {
            var id = new AutoUpdaterId(@event.BoundedContext, @event.Id.Tenant);

            var finish = new FinishAutoUpdate(id, @event.Name, DateTimeOffset.UtcNow);
            await commandPublisher.PublishAsync(finish, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
