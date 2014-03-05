using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Sample.Collaboration.Users.Commands;

namespace NMSD.Cronus.Sample.Collaboration.Users
{
    public class UserAppService : AggregateRootApplicationService<User>,
        IMessageHandler<CreateUser>,
        IMessageHandler<RenameUser>
    {
        public void Handle(RenameUser command)
        {
            Repository.Update<User>(command.Id, command, user => user.Rename(command.FirstName, command.LastName));
        }

        public void Handle(CreateUser command)
        {
            Repository.Save(new User(command.Id, command.Email), command);
        }
    }
}