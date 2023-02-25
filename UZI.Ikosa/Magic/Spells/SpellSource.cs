using System;
using System.Collections.Generic;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic
{
    /// <summary>
    /// Intended to be used as a source for adjuncts, reach effects and interactions [InteractData, ICore, IMagicAura]
    /// </summary>
    [Serializable]
    public class SpellSource : MagicPowerActionSource
    {
        #region Construction
        public SpellSource(ICasterClass caster, int spellLevel, int slotLevel, bool spontaneous, ISpellDef spellDef)
            : base(caster, spellLevel, spellDef)
        {
            _SlotLevel = slotLevel;
            _IsSpontaneous = spontaneous;
        }
        #endregion

        #region state
        private int _SlotLevel;
        private bool _IsSpontaneous;
        #endregion

        // NOTE: for magic items, define an echo caster class that provides fixed properties
        public ICasterClass CasterClass
            => PowerClass as ICasterClass;

        /// <summary>Slot Level from which this spell was cast</summary>
        public int SlotLevel => _SlotLevel;

        public bool IsSpontaneous => _IsSpontaneous;
        public ISpellDef SpellDef => MagicPowerActionDef as ISpellDef;

        public override void UsePower()
        {
            // TODO: slot use...
        }

        public override string DisplayName
            => SpellDef.DisplayName;

        public SpellSourceInfo ToSpellSourceInfo()
        {
            var _info = ToInfo<SpellSourceInfo>();
            _info.SlotLevel = SlotLevel;
            _info.IsSpontaneous = IsSpontaneous;
            return _info;
        }
    }
}
