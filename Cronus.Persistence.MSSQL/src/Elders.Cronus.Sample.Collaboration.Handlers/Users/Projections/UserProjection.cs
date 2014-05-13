using Elders.Cronus.DomainModelling;
using Elders.Cronus.Sample.Collaboration.Users.DTOs;
using Elders.Cronus.Sample.Collaboration.Users.Events;

namespace Elders.Cronus.Sample.Collaboration.Projections
{
    public class UserProjection : IHaveNhibernateSession,
        IMessageHandler<UserCreated>
    {
        public NHibernate.ISession Session { get; set; }
        public void Handle(UserCreated message)
        {
            var usr = new User();
            usr.Id = message.Id.Id;
            usr.Email = message.Email;
           // Session.Save(usr);
        }
    }
}