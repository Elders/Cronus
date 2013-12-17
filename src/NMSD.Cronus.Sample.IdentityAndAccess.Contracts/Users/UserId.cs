using System;
using System.Runtime.Serialization;
using NMSD.Cronus.Core.Cqrs;

namespace NMSD.Cronus.Sample.IdentityAndAccess.Users
{
    [DataContract(Name = "68cb3c79-0d0e-40d4-8dd5-0a49a361ecdd")]
    public class UserId : AggregateRootId
    {
        UserId() { }
        public UserId(Guid id) : base(id) { }
    }
}
