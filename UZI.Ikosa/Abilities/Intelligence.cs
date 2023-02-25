using System;
using Uzi.Core;
using System.Linq;
using Uzi.Ikosa.Deltas;

namespace Uzi.Ikosa.Abilities
{
    [Serializable]
    public class Intelligence : CastingAbilityBase
    {
        public Intelligence(int seedValue)
            : base(seedValue, MnemonicCode.Int)
        {
        }

        /// <summary>
        /// Non-Ability Constructor
        /// </summary>
        public Intelligence()
            : base(MnemonicCode.Int)
        {
        }

        public int BonusLanguages
        {
            get
            {
                var _eff = ValueAtPowerLevel(0, null);
                _eff -= Deltas.Where(_del => ((_del.Source as Type) == typeof(Enhancement)) && _del.Enabled).Sum(_del => _del.Value);
                return Math.Max((int)Math.Floor(((decimal)_eff - 10.0M) / 2.0M), 0);
            }
        }

        public int AdvancementSkillPointModifier(int powerLevel)
        {
            var _eff = ValueAtPowerLevel(powerLevel, new Interaction(null, this, null, new SkillPointCalc()));
            _eff -= Deltas.Where(_del => ((_del.Source as Type) == typeof(Enhancement)) && _del.Enabled).Sum(_del => _del.Value);
            return (int)Math.Floor(((decimal)_eff - 10.0M) / 2.0M);
        }

        public override void Boost(int powerLevel)
        {
            base.Boost(powerLevel);
            // TODO: after boost, if necessary, rollback skillpoints for power die at or above this level, then 'recalc'
        }

        public override void Unboost(int powerLevel)
        {
            base.Unboost(powerLevel);
            // TODO: after unBoost, if necessary, rollback skillpoints for power die at or above this level, then 'recalc'
        }

        public class SkillPointCalc : InteractData
        {
            public SkillPointCalc()
                : base(null)
            {
            }
        }
    }
}
