using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class UserController : Adjunct
    {
        public UserController(string userName)
            : base(typeof(UserController))
        {
            _UserName = userName;
        }

        private string _UserName;

        public string UserName { get { return _UserName; } set { _UserName = value; } }
        public override bool IsProtected => true;

        public override object Clone()
            => new UserController(UserName);
    }

    public static class UserControllerExtensions
    {
        public static bool CanUserControl(this Creature critter, string userName)
            => critter.Adjuncts.OfType<UserController>()
            .Any(_uc => _uc.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));

        public static IEnumerable<string> GetUsers(this Creature critter)
            => critter.Adjuncts.OfType<UserController>()
            .Select(_uc => _uc.UserName);
    }
}
