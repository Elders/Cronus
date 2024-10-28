using Elders.Cronus.EventStore.AutoUpdater.Events;
using System;

namespace Elders.Cronus.EventStore.AutoUpdater;

public class AutoUpdater : AggregateRoot<AutoUpdaterState>
{
    AutoUpdater() { }

    public AutoUpdater(AutoUpdaterId id, uint majorVersion, string bc, DateTimeOffset timestamp)
    {
        Apply(new AutoUpdaterInitialized(id, majorVersion, bc, timestamp));
    }

    public void TriggerUpdate(uint versionToUpdateTo, DateTimeOffset timestamp)
    {
        if (ShouldTriggerUpdate(versionToUpdateTo)) // TODO: Add more meaningful checks
        {
            Apply(new AutoUpdateTriggered(state.Id, state.CurrentVersion, versionToUpdateTo, state.BoundedContext, timestamp));
        }
    }

    public void FinishUpdate(DateTimeOffset timestamp)
    {
        if (state.VersionStatus.Equals(AutoUpdaterStatus.Triggered))
        {
            Apply(new AutoUpdateFinished(state.Id, state.CurrentVersion, state.BoundedContext, timestamp));
        }
    }

    public void FailUpdate(DateTimeOffset timestamp)
    {
        if (state.VersionStatus.Equals(AutoUpdaterStatus.Triggered))
        {
            Apply(new AutoUpdateFailed(state.Id, state.CurrentVersion, state.BoundedContext, timestamp));
        }
    }

    private bool ShouldTriggerUpdate(uint versionToUpdateTo)
    {
        if (state.VersionStatus.Equals(AutoUpdaterStatus.Failed))
        {
            if (versionToUpdateTo == state.CurrentVersion)
                return true;
            else
                return false;
        }
        else if (versionToUpdateTo > state.CurrentVersion &&
            state.VersionStatus == AutoUpdaterStatus.None ||
            state.VersionStatus == AutoUpdaterStatus.Finished)
            return true;
        else
            return false;
    }
}
