namespace Elders.Cronus
{
    public class CronusAssembly { }

    public interface ISystemAppService : IApplicationService, ISystemHandler { }

    public interface ISystemProjection : IProjection, ISystemHandler { }

    public interface INonVersionableProjection
    {
        string GetHash() => "ver";

        int GetRevision() => 1;
    }

    public interface INonRebuildableProjection { }

    public interface ISystemHandler : IMessageHandler { }

    public interface ISystemPort : IPort, ISystemHandler { }

    public interface ISystemTrigger : ITrigger, ISystemHandler { }

    public interface ISystemEventStoreIndexHandler : ISystemHandler { }

    public interface ISystemSaga : ISaga, ISystemHandler { }

    public interface ISystemMessage : IMessage { }

    public interface ISystemEvent : IEvent, ISystemMessage { }

    public interface ISystemCommand : ICommand, ISystemMessage { }

    public interface ISystemSignal : ISignal, ISystemMessage { }

    public interface ISystemScheduledMessage : IScheduledMessage, ISystemMessage { }
}
