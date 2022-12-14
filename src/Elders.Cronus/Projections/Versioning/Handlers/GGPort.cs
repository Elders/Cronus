using Elders.Cronus.MessageProcessing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections.Versioning.Handlers
{
    [DataContract(Name = "49ff4195-0a8b-43f2-a55e-6e76a91d7bf0")]
    public class GGPort : ISystemPort,
        IEventHandler<NewProjectionVersionIsNowLive>
    {
        private readonly IProjectionStore projectionStore;
        private readonly CronusContext cronusContext;
        private readonly ILogger<GGPort> logger;

        public GGPort(IProjectionStore projectionStore, CronusContext cronusContext, ILogger<GGPort> logger)
        {
            this.projectionStore = projectionStore;
            this.cronusContext = cronusContext;
            this.logger = logger;
        }

        public async Task HandleAsync(NewProjectionVersionIsNowLive @event)
        {
            var projectionType = @event.ProjectionVersion.ProjectionName.GetTypeByContract();
            if (projectionType.IsRebuildableProjection())
            {
                var id = Urn.Parse($"urn:cronus:{@event.ProjectionVersion.ProjectionName}");

                object projectionPlain = cronusContext.ServiceProvider.GetRequiredService(projectionType);
                if (projectionPlain is IAmEventSourcedProjectionFast projectionFast)
                {
                    try
                    {
                        List<Task> tasks = new List<Task>();
                        var asyncCommits = projectionStore.EnumerateProjectionAsync(@event.ProjectionVersion, id).ConfigureAwait(false);
                        await foreach (var commit in asyncCommits)
                        {
                            var currentTask = projectionFast.ReplayEventAsync(commit.Event);
                            if (tasks.Count > 100)
                            {
                                Task finished = await Task.WhenAny(tasks).ConfigureAwait(false);
                                tasks.Remove(finished);
                            }
                        }

                        await Task.WhenAll(tasks).ConfigureAwait(false);
                        await projectionFast.OnReplayCompletedAsync();

                    }
                    catch (Exception ex)
                    {
                        logger.ErrorException(ex, () => "Error while replaying projection.");
                    }
                }
                else if (projectionPlain is IAmEventSourcedProjection projection)
                {
                    try
                    {
                        var asyncCommits = projectionStore.EnumerateProjectionAsync(@event.ProjectionVersion, id).ConfigureAwait(false);
                        await foreach (var commit in asyncCommits)
                        {
                            await projection.ReplayEventAsync(commit.Event).ConfigureAwait(false);
                        }

                        await projection.OnReplayCompletedAsync();

                    }
                    catch (Exception ex)
                    {
                        logger.ErrorException(ex, () => "Error while replaying projection.");
                    }
                }
            }
        }
    }

    interface IRebuildableProjection
    {
        void Rebuild(IEnumerable<IEvent> events);
    }

    public static class GGExtensions
    {
        public static bool IsRebuildableProjection(this Type projectionType)
        {
            return
                typeof(IAmEventSourcedProjection).IsAssignableFrom(projectionType) &&
                typeof(IProjectionDefinition).IsAssignableFrom(projectionType) == false;
        }
    }
}
