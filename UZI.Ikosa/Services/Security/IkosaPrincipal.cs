using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Services
{
    public class IkosaPrincipal : IPrincipal
    {
        public IkosaPrincipal(IIdentity identity, UserDefinition user)
        {
            _Identity = identity;
            _User = user;
        }

        #region data
        private IIdentity _Identity;
        private UserDefinition _User;
        #endregion

        public IList<CreatureLoginInfo> GetCreatureLogins()
            => LoginService.GetUserInfo(_User.UserName)?.CreatureInfos.ToList()
            ?? [];

        // IPrincipal Members
        public IIdentity Identity => _Identity;
        public bool IsInRole(string role)
            => _User.IsMasterUser
            || (LoginService.GetUserInfo(_User.UserName)?.CreatureInfos.Any(_ci => _ci.ID.ToString().Equals(role)) ?? false);
    }
}
