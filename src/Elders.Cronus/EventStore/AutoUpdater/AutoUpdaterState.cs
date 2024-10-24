using Elders.Cronus.EventStore.AutoUpdater.Events;

namespace Elders.Cronus.EventStore.AutoUpdater
{
    public class AutoUpdaterState : AggregateRootState<AutoUpdater, AutoUpdaterId>
    {
        public AutoUpdaterState() { }

        public override AutoUpdaterId Id { get; set; }

        public AutoUpdaterStatus VersionStatus { get; set; }

        public uint CurrentVersion { get; set; } // only the major version is here ,because this AR is keeping track of the entire cronus Ecosystem, not the individual packets

        public string BoundedContext { get; set; }

        public void When(AutoUpdaterInitialized @event)
        {
            Id = @event.Id;
            VersionStatus = AutoUpdaterStatus.None;

            CurrentVersion = @event.InitialVersion;
            BoundedContext = @event.BoundedContext;
        }

        public void When(AutoUpdateTriggered @event)
        {
            Id = @event.Id;
            CurrentVersion = @event.CurrentVersion;

            VersionStatus = AutoUpdaterStatus.Triggered;
        }

        public void When(AutoUpdateFinished @event)
        {
            Id = @event.Id;
            VersionStatus = AutoUpdaterStatus.Finished;

        }
        public void When(AutoUpdateFailed @event)
        {
            Id = @event.Id;
            VersionStatus = AutoUpdaterStatus.Failed;
        }
    }
}
