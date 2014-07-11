using Elders.Cronus.DomainModelling;
using Elders.Cronus.Sample.Collaboration.Users.Commands;

namespace Elders.Cronus.Sample.Collaboration.Users
{
    public class UserAppService : AggregateRootApplicationService<User>,
        IMessageHandler<CreateUser>,
        IMessageHandler<RenameUser>,
        IMessageHandler<ChangeEmail>
    {
        public void Handle(RenameUser command)
        {
            // Explicit
            var ar = Repository.Load<User>(command.Id);
            ar.Rename(command.FirstName, command.LastName);
            Repository.Save(ar);
        }

        public void Handle(ChangeEmail command)
        {
            // Implicit
            Update(command.Id, user => user.ChangeEmail(command.NewEmail));
        }

        public void Handle(CreateUser command)
        {
            Repository.Save(new User(command.Id, command.Email));
        }
    }
}