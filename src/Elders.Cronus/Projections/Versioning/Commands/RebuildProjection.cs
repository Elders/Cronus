using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "9b309ab7-3fac-4cf8-97e2-d2e74fbaa623")]
    public class RebuildProjection : ICommand
    {
        RebuildProjection() { }

        public RebuildProjection(ProjectionVersionManagerId id, string hash)
        {
            if (ReferenceEquals(null, id)) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(hash)) throw new ArgumentNullException(nameof(hash));

            Id = id;
            Hash = hash;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public string Hash { get; private set; }
    }
}
