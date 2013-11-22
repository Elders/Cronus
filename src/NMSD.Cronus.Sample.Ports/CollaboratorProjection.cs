using System;
using Cronus.Core.Eventing;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;

namespace NMSD.Cronus.Sample.Ports
{
    public class CollaboratorProjection : IEventHandler,
        IEventHandler<NewCollaboratorCreated>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CollaboratorProjection));

        public void Handle(NewCollaboratorCreated evnt)
        {
            log.Info(evnt);
        }
    }
}
