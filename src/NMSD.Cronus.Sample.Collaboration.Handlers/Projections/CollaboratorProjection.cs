using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;

namespace NMSD.Cronus.Sample.Collaboration.Projections
{
    public static class MyClass
    {
        public static int count = 0;
    }

    public class CollaboratorProjection :
        IMessageHandler<NewCollaboratorCreated>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CollaboratorProjection));

        public void Handle(NewCollaboratorCreated message)
        {
            ++MyClass.count;
            log.Info(MyClass.count);
        }
    }
}