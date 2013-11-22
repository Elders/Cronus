using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Core.Commanding;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators.Commands
{
    public class RenameCollaborator : ICommand
    {
        public CollaboratorId Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
