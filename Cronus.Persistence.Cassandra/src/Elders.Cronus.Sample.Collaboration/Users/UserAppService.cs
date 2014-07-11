using Elders.Cronus.DomainModelling;
using Elders.Cronus.Sample.Collaboration.Users.Commands;

namespace Elders.Cronus.Sample.Collaboration.Users
{
    public class UserAppService : AggregateRootApplicationService<User>,
        IMessageHandler<CreateUser>,
        IMessageHandler<RenameUser>
    {
        public void Handle(RenameUser command)
        {
            Update(command.Id, user => user.Rename(command.FirstName, command.LastName));
        }

        public void Handle(CreateUser command)
        {
            Repository.Save(new User(command.Id, command.Email));
        }
    }
}