using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class PreparedSpellSlot : SpellSlot
    {
        #region data
        private SpellSource _PrepSpell;
        #endregion

        /// <summary>This may be modified by meta-magic</summary>
        public SpellSource PreparedSpell
        {
            get => _PrepSpell;
            set => _PrepSpell = value;
        }

        /// <summary>Recharging a slot with prepared spell will abandon the spell, as well as resetting the last charged time</summary>
        public override void RechargeSlot(double currentTime)
        {
            _PrepSpell = null;
            base.RechargeSlot(currentTime);
        }

        public override void UseSlot(double? currentTime)
        {
            _PrepSpell = null;
            base.UseSlot(currentTime);
        }

        public override SpellSlotInfo ToSpellSlotInfo(int slotIndex, double currentTime)
        {
            var _info = ToInfo<PreparedSpellSlotInfo>(slotIndex, currentTime);
            if (PreparedSpell != null)
                _info.SpellSource = PreparedSpell.ToSpellSourceInfo();
            return _info;
        }
    }
}
