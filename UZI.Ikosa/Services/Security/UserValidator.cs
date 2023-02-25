using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

namespace Uzi.Ikosa.Services
{
    public class UserValidator : UserNamePasswordValidator
    {

        private static UserDefinitionCollection _Users = null;
        public static UserDefinitionCollection UserDefinitions
        {
            get { return _Users; }
            set { _Users = value; }
        }

        public override void Validate(string userName, string password)
        {
            if (_Users != null)
            {
                var _user = _Users.GetUser(userName);
                if ((_user?.IsDisabled ?? true) || !_user.Password.Equals(password))
                {
                    throw new SecurityTokenException(@"Unknown user or password");
                }
            }
        }
    }
}
