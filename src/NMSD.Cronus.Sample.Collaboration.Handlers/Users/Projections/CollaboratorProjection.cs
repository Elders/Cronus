using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Sample.Collaboration.Users.DTOs;
using NMSD.Cronus.Sample.Collaboration.Users.Events;

namespace NMSD.Cronus.Sample.Collaboration.Projections
{
    public class UserProjection : IHaveNhibernateSession,
        IMessageHandler<UserCreated>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(UserProjection));

        public NHibernate.ISession Session { get; set; }

        public void Handle(UserCreated message)
        {
            //log.Info(message);
            //Session.Save(new Collaborator() { Id = message.CollaboratorId.Id, Email = message.Email });
        }
    }
}