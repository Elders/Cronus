using Elders.Cronus.EventStore.AutoUpdater.Commands;
using Elders.Cronus.EventStore.AutoUpdater.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.EventStore.AutoUpdater;

public class AutoUpdater : AggregateRoot<AutoUpdaterState>
{
    AutoUpdater() { }

    public AutoUpdater(AutoUpdaterId id, string bc)
    {
        Apply(new AutoUpdaterInitialized(id, bc, DateTimeOffset.UtcNow));
    }

    public void BulkRequestUpdate(IEnumerable<SingleAutoUpdate> updates)
    {
        var ordered = updates.OrderBy(x => x.Sequence);
        foreach (var update in ordered)
        {
            RequestUpdate(update.Name, update.Sequence, update.IsSystem);
        }
    }

    public void RequestUpdate(string name, uint sequence, bool isSystem)
    {
        if (state.AutoUpdateExists(name) == false)
        {
            Apply(new AutoUpdateRequested(state.Id, name, sequence, state.BoundedContext, isSystem, DateTimeOffset.UtcNow));
        }

        HandleTriggerOfAutoUpdates(AutoUpdaterStatus.Requested, name, sequence);
    }

    public void FinishUpdate(string name)
    {
        if (ExistsInTriggeredState(name, out var update))
        {
            Apply(new AutoUpdateFinished(state.Id, update.Name, update.Sequence, state.BoundedContext, update.IsSystemUpdate, DateTimeOffset.UtcNow));

            AutoUpdate autoUpdate = state.GetByName(name);
            HandleTriggerOfAutoUpdates(AutoUpdaterStatus.Finished, name, autoUpdate.Sequence);
        }
    }

    public void FailUpdate(string name)
    {
        if (ExistsInTriggeredState(name, out var update))
        {
            Apply(new AutoUpdateFailed(state.Id, update.Name, update.Sequence, state.BoundedContext, update.IsSystemUpdate, DateTimeOffset.UtcNow));
        }
    }

    private void HandleTriggerOfAutoUpdates(AutoUpdaterStatus currentStatus, string name, uint currentExecutionSequence) // TODO: failed status?
    {
        if (currentStatus.Equals(AutoUpdaterStatus.Requested) || currentStatus.Equals(AutoUpdaterStatus.Finished))
        {
            if (state.ThereIsAutoUpdateInProgress() == false)
            {
                AutoUpdate nextUpdate = state.GetNextToExecute(currentExecutionSequence);
                if (nextUpdate is not null)
                {
                    Apply(new AutoUpdateTriggered(state.Id, nextUpdate.Name, nextUpdate.Sequence, state.BoundedContext, nextUpdate.IsSystemUpdate, DateTimeOffset.UtcNow));
                }
            }
        }
    }

    private bool ExistsInTriggeredState(string name, out AutoUpdate theUpdate)
    {
        theUpdate = null;

        AutoUpdate autoUpdate = state.GetByName(name);
        if (autoUpdate is not null)
        {
            if (autoUpdate.Status.Equals(AutoUpdaterStatus.Triggered))
            {
                theUpdate = autoUpdate;
                return true;
            }
        }
        return false;
    }
}
