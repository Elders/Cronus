using System.Runtime.Serialization;
using System.Text;

namespace Elders.Cronus.Projections.Snapshotting
{
    [DataContract(Name = "18b8d888-5ac5-4441-99f8-32ffa38df8f1")]
    public class Snapshot : ISnapshot
    {
        public Snapshot() { }
        public Snapshot(IBlobId id, string projectionName, object state, int revision)
        {
            Id = id;
            ProjectionName = projectionName;
            State = state;
            Revision = revision;
        }

        [DataMember(Order = 1)]
        public IBlobId Id { get; private set; }

        [DataMember(Order = 2)]
        public string ProjectionName { get; private set; }

        [DataMember(Order = 3)]
        public object State { get; set; }

        [DataMember(Order = 4)]
        public int Revision { get; private set; }

        public void InitializeState(object state)
        {
            State = state;
        }

        public override string ToString()
        {
            return $"{Encoding.UTF8.GetString(Id.RawId)}:{ProjectionName}";
        }
    }
}
