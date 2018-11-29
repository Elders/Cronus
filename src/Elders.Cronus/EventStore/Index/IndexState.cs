using System.Runtime.Serialization;

namespace Elders.Cronus.EventStore.Index
{
    [DataContract(Name = "fda4a09e-3bd6-46c2-b104-514e6b7166f8")]
    public class IndexStatus
    {
        [DataMember(Order = 1)]
        string status;

        public IndexStatus() { }

        private IndexStatus(string status)
        {
            this.status = status;
        }

        public bool IsPresent()
        {
            return this.status == Present.status;
        }

        public bool IsBuilding()
        {
            return this.status == Building.status;
        }

        public bool IsNotBuilding()
        {
            return IsBuilding() == false;
        }

        public bool IsNotPresent()
        {
            return IsPresent() == false;
        }

        public static IndexStatus NotPresent = new IndexStatus("notpresent");

        public static IndexStatus Building = new IndexStatus("building");

        public static IndexStatus Present = new IndexStatus("present");

        public static IndexStatus Parse(string status)
        {
            switch (status)
            {
                case "building": return Building;
                case "present": return Present;
                default: return NotPresent;
            }
        }

        public override string ToString()
        {
            return status;
        }

        public static implicit operator string(IndexStatus d)
        {
            return d.status;
        }
    }
}
