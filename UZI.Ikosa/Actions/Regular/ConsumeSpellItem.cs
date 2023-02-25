using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class ConsumeSpellItem : ConsumeItem, IPowerUse<SpellSource>
    {
        public ConsumeSpellItem(IConsumableItem consumable, SpellSource spellSource, ISpellMode spellMode, string orderKey)
            : base(consumable, orderKey)
        {
            _SpellSource = spellSource;
            _Root = spellMode;
        }

        #region state
        private SpellSource _SpellSource;
        private ISpellMode _Root;
        private PowerAffectTracker _Tracker;
        #endregion

        public void ApplyPower(PowerApplyStep<SpellSource> step)
        {
            _Root.ApplySpell(step as PowerApplyStep<SpellSource>);
        }

        public void ActivatePower(PowerActivationStep<SpellSource> step)
        {
            _Root.ActivateSpell(step as PowerActivationStep<SpellSource>);
        }

        public ICapabilityRoot CapabilityRoot => _Root;

        public SpellSource PowerActionSource => _SpellSource;

        public PowerAffectTracker PowerTracker
            => _Tracker ??= new PowerAffectTracker();
    }
}