using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "25e39039-8b05-411e-b62b-161e5ea91902")]
    public class RebuildProjectionCommand : ISystemCommand
    {
        RebuildProjectionCommand() { }

        public RebuildProjectionCommand(ProjectionVersionManagerId id, string hash)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(hash)) throw new ArgumentNullException(nameof(hash));

            Id = id;
            Hash = hash;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public string Hash { get; private set; }

        public override string ToString()
        {
            return $"Rebuild projection with hash `{Hash}`. {nameof(ProjectionVersionManagerId)}: `{Id}`";
        }
    }
}
