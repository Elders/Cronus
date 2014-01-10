using NMSD.Cronus.Messaging;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;

namespace NMSD.Cronus.Sample.Collaboration.Projections
{
    public class CollaboratorProjection :
        IMessageHandler<NewCollaboratorCreated>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CollaboratorProjection));

        public void Handle(NewCollaboratorCreated message)
        {
            log.Info(message);
        }
    }
}