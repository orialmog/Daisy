namespace Ancestry.Daisy.Tests.Daisy.Component.Controllers
{
    using System;
    using System.Linq;

    using Ancestry.Daisy.Statements;
    using Ancestry.Daisy.Tests.Daisy.Component.Domain;

    public class UserController : StatementController<User>
    {
        [Matches(@"Has\s+Account(?:\s*=>\s*(.*))?")]
        public bool HasAccount(Func<Account, bool> proceed, string name = null)
        {
            Trace("Has {0} accounts", Scope.Accounts.Count);
            foreach (var entry in Scope.Accounts)
            {
                if (proceed(entry))
                {
                    if(name != null)
                        this.Attachments[name] = entry;
                    return true;
                }
            }
            return false;
        }
        public bool AllAccounts(Func<Account,bool> procced)
        {
            return Scope.Accounts.Any(procced);
        }

        public bool IsActive()
        {
            return Scope.IsActive;
        }

        public override string ToString()
        {
            return "User";
        }
    }
}
