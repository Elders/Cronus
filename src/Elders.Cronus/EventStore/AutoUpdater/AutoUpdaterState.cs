using Elders.Cronus.EventStore.AutoUpdater;
using Elders.Cronus.EventStore.AutoUpdater.Events;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.EventStore.AutoUpdater
{
    public class AutoUpdaterState : AggregateRootState<AutoUpdater, AutoUpdaterId>
    {
        private readonly List<AutoUpdate> _updatesHistory;
        private readonly List<AutoUpdate> _updatesToExecute;

        public AutoUpdaterState()
        {
            _updatesHistory = new List<AutoUpdate>();
            _updatesToExecute = new List<AutoUpdate>();
        }

        public override AutoUpdaterId Id { get; set; }

        public AutoUpdaterStatus VersionStatus { get; set; }

        public string BoundedContext { get; set; }

        public void When(AutoUpdaterInitialized @event)
        {
            Id = @event.Id;
            BoundedContext = @event.BoundedContext;
        }

        public void When(AutoUpdateRequested @event)
        {
            var autoUpdate = GetByName(@event.Name);
            if (autoUpdate is null)
            {
                autoUpdate = new AutoUpdate(@event.Name, @event.Sequence, AutoUpdaterStatus.Requested, @event.IsSystem);

                _updatesHistory.Add(autoUpdate);
                _updatesToExecute.Add(autoUpdate);
            }
        }

        public void When(AutoUpdateTriggered @event)
        {
            var autoUpdate = GetByName(@event.Name);
            if (autoUpdate is not null)
            {
                autoUpdate.Status = AutoUpdaterStatus.Triggered;

                _updatesToExecute.Remove(autoUpdate);
            }
        }

        public void When(AutoUpdateFinished @event)
        {
            var autoUpdate = GetByName(@event.Name);
            if (autoUpdate is not null)
            {
                autoUpdate.Status = AutoUpdaterStatus.Finished;

                _updatesToExecute.Remove(autoUpdate);
            }
        }

        public void When(AutoUpdateFailed @event)
        {
            var autoUpdate = GetByName(@event.Name);
            if (autoUpdate is not null)
            {
                autoUpdate.Status = AutoUpdaterStatus.Failed;

                _updatesToExecute.Remove(autoUpdate); // TODO: think for retries?
            }
        }

        public AutoUpdate GetByName(string name) => _updatesHistory.Where(x => x.Name.Equals(name)).SingleOrDefault();

        public bool AutoUpdateExists(string name) => _updatesHistory.Where(x => x.Name.Equals(name)).SingleOrDefault() is not null;

        public bool ThereIsAutoUpdateInProgress() => _updatesHistory.Where(x => x.Status.Equals(AutoUpdaterStatus.Triggered)).SingleOrDefault() is not null; // TODO: in future we might execute in parallel

        public AutoUpdate GetNextToExecute(uint currentExecutionSequence) => _updatesToExecute.Where(x => x.Sequence >= currentExecutionSequence).MinBy(x => x.Sequence);
    }
}

public record AutoUpdate
{
    public AutoUpdate(string name, uint sequence, AutoUpdaterStatus status, bool isSystemUpdaye)
    {
        Name = name;
        Sequence = sequence;
        Status = status;
        IsSystemUpdate = isSystemUpdaye;
    }

    public string Name { get; set; }

    public uint Sequence { get; set; }

    public AutoUpdaterStatus Status { get; set; }

    public bool IsSystemUpdate { get; set; } // indicates that this is a cronus update or something custom someone wrote.
}
