using System;
using Uzi.Core;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class AbilityDamageRule : DamageRule
    {
        public AbilityDamageRule(string key, Range range, string mnemonic, string name)
            : base(key, range, false, name)
        {
            _Mnemonic = mnemonic;
        }

        private string _Mnemonic;

        public string AbilityMnemonic { get { return _Mnemonic; } }
    }
}
