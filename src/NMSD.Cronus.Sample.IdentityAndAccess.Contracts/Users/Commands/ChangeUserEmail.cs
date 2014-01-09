using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Core.Commanding;

namespace NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands
{
    [DataContract(Name = "645b65a5-9381-44c5-9d11-7c67c1e3ce34")]
    public class ChangeUserEmail : ICommand
    {
        ChangeUserEmail() { }

        public ChangeUserEmail(UserId id, string oldEmail, string newEmail)
        {
            NewEmail = newEmail;
            OldEmail = oldEmail;
            Id = id;
        }

        [DataMember(Order = 1)]
        public UserId Id { get; private set; }

        [DataMember(Order = 2)]
        public string OldEmail { get; private set; }

        [DataMember(Order = 3)]
        public string NewEmail { get; private set; }

        [DataMember(Order = 4)]
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return this.ToString("Change old user email '{0}' with '{1}'. {2}", OldEmail, NewEmail, Id);
        }
    }
}
