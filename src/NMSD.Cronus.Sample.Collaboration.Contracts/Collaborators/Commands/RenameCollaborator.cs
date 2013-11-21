using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators.Commands
{
    public class RenameCollaborator
    {
        public CollaboratorId Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
