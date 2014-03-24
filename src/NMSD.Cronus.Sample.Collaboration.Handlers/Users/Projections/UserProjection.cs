using System;
using System.Collections.Generic;
using NHibernate;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Sample.Collaboration.Users.DTOs;
using NMSD.Cronus.Sample.Collaboration.Users.Events;

namespace NMSD.Cronus.Sample.Collaboration.Projections
{
    public class UserProjection : IHaveNhibernateSession,
        IMessageHandler<UserCreated>
    {
        public NHibernate.ISession Session { get; set; }
        public void Handle(UserCreated message)
        {
            var usr = new User();
            usr.Id = message.CollaboratorId.Id;
            usr.Email = message.Email;
            Session.Save(usr);
        }
    }
}