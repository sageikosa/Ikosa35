using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class DamageData : ICloneable
    {
        public DamageData(int amount, bool nonLethal, string extra, int minGroup)
        {
            // any unitary damage must be at least one unit...
            _Amount = amount;
            _NonLethal = nonLethal;
            _Extra = extra;
            _MinGroup = minGroup;
        }

        #region data
        private bool _NonLethal;
        private int _Amount;
        private string _Extra;
        private int _MinGroup;
        #endregion

        public bool IsNonLethal { get => _NonLethal; set => _NonLethal = value; }
        public int Amount { get => _Amount; set => _Amount = value; }
        public string Extra { get => _Extra; set => _Extra = value; }
        public int MinGroup => _MinGroup;

        protected DInfo ToBaseDamageInfo<DInfo>()
            where DInfo : DamageInfo, new()
        {
            return new DInfo
            {
                Amount = Amount,
                Extra = Extra
            };
        }

        public virtual DamageInfo ToDamageInfo()
        {
            if (IsNonLethal)
                return ToBaseDamageInfo<NonLethalDamageInfo>();
            return ToBaseDamageInfo<LethalDamageInfo>();
        }

        public virtual object Clone()
            => new DamageData(Amount, IsNonLethal, Extra, MinGroup);
    }
}
