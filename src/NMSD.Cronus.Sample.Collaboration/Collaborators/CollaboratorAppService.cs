using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;

namespace NMSD.Cronus.Sample.Collaboration.Collaborators
{
    public class CollaboratorAppService
    {
        public dynamic EventStore { get; set; }

        private void UpdateAggregate(IAggregateRootId id, Action<Collaborator> updateAr)
        {
            var events = EventStore.LoadEvents(id);
            var ar = AggregateRootFactory.BuildFromHistory<Collaborator>(events);
            updateAr(ar);
            EventStore.Publish(ar.UncommittedEvents);
        }

        public void Handle(RenameCollaborator cmd)
        {
            UpdateAggregate(cmd.Id, ar => ar.Rename(cmd.FirstName, cmd.LastName));
        }
    }
}
