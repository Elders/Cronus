using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "e311e645-79b5-47b4-8711-2964bdee9d0e")]
    public class SnapshotStatus : ValueObject<SnapshotStatus>
    {
        SnapshotStatus() { }
        SnapshotStatus(string status)
        {
            Status = status;
        }

        [DataMember(Order = 1)]
        public string Status { get; private set; }

        public readonly static SnapshotStatus Completed = new("completed");
        public readonly static SnapshotStatus Canceled = new("canceled");
        public readonly static SnapshotStatus Failed = new("failed");
        public readonly static SnapshotStatus Running = new("running");

        public bool IsRunning => Status == Running.Status;
    }
}
