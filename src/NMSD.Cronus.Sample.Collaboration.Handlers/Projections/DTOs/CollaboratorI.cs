using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus.Sample.Collaboration.Projections.DTOs
{
    public class Collaborator
    {
        public virtual Guid Id { get; set; }
        public virtual string Email { get; set; }
    }
}
