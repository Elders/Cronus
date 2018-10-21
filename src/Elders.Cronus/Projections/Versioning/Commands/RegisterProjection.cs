using System;
using System.Runtime.Serialization;

namespace Elders.Cronus.Projections.Versioning
{
    [DataContract(Name = "d54d6def-2c33-4d19-9009-26ae357d6fc2")]
    public class RegisterProjection : ICommand
    {
        RegisterProjection() { }

        public RegisterProjection(ProjectionVersionManagerId id, string hash, string tenant)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(hash)) throw new ArgumentNullException(nameof(hash));
            if (string.IsNullOrEmpty(tenant)) throw new ArgumentNullException(nameof(tenant));

            Id = id;
            Hash = hash;
            Tenant = tenant;
        }

        [DataMember(Order = 1)]
        public ProjectionVersionManagerId Id { get; private set; }

        [DataMember(Order = 2)]
        public string Hash { get; private set; }

        [DataMember(Order = 3)]
        public string Tenant { get; private set; }

        public override string ToString()
        {
            return $"Register projection with `{Hash}`. {nameof(ProjectionVersionManagerId)}: `{Id}`.";
        }
    }
}
