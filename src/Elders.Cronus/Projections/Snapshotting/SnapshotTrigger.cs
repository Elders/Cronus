using Elders.Cronus.MessageProcessing;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elders.Cronus.Projections.Snapshotting
{
    [DataContract(Name = "ac1c8d62-5cb2-42ab-b6ab-9fee1e21a275")]
    public class SnapshotTrigger : ISystemTrigger,
       ISignalHandle<ProjectionCommitSignaled>
    {
        private readonly ISnapshotDistributor distributor;

        public SnapshotTrigger(ISnapshotDistributor distributor)
        {
            this.distributor = distributor;
        }

        public async Task HandleAsync(ProjectionCommitSignaled signal)
        {
            await distributor.CreateSnapshot(signal.ProjectionType, signal.Version, signal.ProjectionId, signal.Tenant).ConfigureAwait(false);
        }
    }
}
