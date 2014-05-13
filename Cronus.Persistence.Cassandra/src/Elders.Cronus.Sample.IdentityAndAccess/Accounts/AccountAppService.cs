using Elders.Cronus.DomainModelling;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;

namespace Elders.Cronus.Sample.IdentityAndAccess.Accounts
{
    public class AccountAppService : AggregateRootApplicationService<Account>,
        IMessageHandler<RegisterAccount>,
        IMessageHandler<ChangeAccountEmail>
    {

        public void Handle(RegisterAccount command)
        {
            Repository.Save(new Account(command.Id, command.Email), command);
        }

        public void Handle(ChangeAccountEmail command)
        {
            Repository.Update<Account>(command, user => user.ChangeEmail(command.OldEmail, command.NewEmail));
        }
    }
}
