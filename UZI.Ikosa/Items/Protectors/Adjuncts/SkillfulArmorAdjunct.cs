using System;
using Uzi.Core;
using Uzi.Ikosa.Deltas;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// Base class for armor adjuncts that improve some skill of the wearer
    /// </summary>
    [Serializable]
    public abstract class SkillfulArmorAdjunct : ProtectorAdjunct
    {
        protected SkillfulArmorAdjunct(object source, int amount)
            : base(source, 0, ProtectorAdjunct.SkillBoostCost(amount))
        {
            _Amt = ((amount < 1) ? 1 : (amount > 15) ? 15 : amount);
            _Amount = null;
        }

        #region private data
        private int _Amt;
        private Delta _Amount;
        #endregion

        public Delta Amount { get { return _Amount; } }
        public int AmountVal { get { return _Amt; } }

        public override bool CanUseOnShield { get { return false; } }

        protected abstract Type SkillType();

        protected override void OnSlottedActivate()
        {
            Protector.CreaturePossessor.Skills[SkillType()].Deltas.Add(_Amount);
        }

        protected override void OnSlottedDeActivate()
        {
            if (_Amount != null)
            {
                _Amount.DoTerminate();
            }
        }

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            base.OnAnchorSet(oldAnchor, oldSetting);
            if (Protector != null)
            {
                _Amount = new Delta(_Amt, typeof(Competence),
                    string.Format(@"Competence via {0}", ((ICoreObject)Protector).Name));
            }
        }
    }
}
