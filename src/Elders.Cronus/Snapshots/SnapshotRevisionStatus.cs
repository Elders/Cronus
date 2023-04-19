using System.Runtime.Serialization;

namespace Elders.Cronus.Snapshots
{
    [DataContract(Name = "e311e645-79b5-47b4-8711-2964bdee9d0e")]
    public class SnapshotRevisionStatus : ValueObject<SnapshotRevisionStatus>
    {
        SnapshotRevisionStatus() { }
        SnapshotRevisionStatus(string status)
        {
            Status = status;
        }

        [DataMember(Order = 1)]
        public string Status { get; private set; }

        public readonly static SnapshotRevisionStatus Completed = new("completed");
        public readonly static SnapshotRevisionStatus Canceled = new("canceled");
        public readonly static SnapshotRevisionStatus Failed = new("failed");
        public readonly static SnapshotRevisionStatus Running = new("running");

        public bool IsRunning => Status == Running.Status;
    }
}
