using NMSD.Cronus.Messaging;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.Collaboration.Projections.DTOs;
using NMSD.Cronus.Sample.Nhibernate.UoW;


namespace NMSD.Cronus.Sample.Collaboration.Projections
{
    public class CollaboratorProjection : IHaveNhibernateSession,
        IMessageHandler<NewCollaboratorCreated>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CollaboratorProjection));
        public NHibernate.ISession Session { get; set; }

        public void Handle(NewCollaboratorCreated message)
        {
            log.Info(message);
            Session.Save(new Collaborator() { Id = message.CollaboratorId.Id, Email = message.Email });
        }
    }
}