using System;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class CharacterStringTarget : AimTarget
    {
        public CharacterStringTarget(string key, string charString)
            : base(key, null)
        {
            _CharStr = charString;
        }

        private readonly string _CharStr;
        public string CharacterString => _CharStr;

        public override AimTargetInfo GetTargetInfo()
            => new CharacterStringTargetInfo
            {
                Key = Key,
                TargetID = Target?.ID,
                CharacterString = CharacterString
            };
    }
}
