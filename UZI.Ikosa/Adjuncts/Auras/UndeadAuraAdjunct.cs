using System;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class UndeadAuraAdjunct : Adjunct, IUndeadAura
    {
        public UndeadAuraAdjunct(object source)
            : base(source)
        {
        }

        #region IUndeadAura Members
        public int PowerLevel
        {
            get
            {
                Creature _critter = Anchor as Creature;
                if (_critter != null)
                {
                    // NOTE: consider only PD that are undead PD?  might prove problematic for templated conversions such as Lich...
                    return _critter.AdvancementLog.NumberPowerDice;
                }
                return 0;
            }
        }

        public AuraStrength Strength
        {
            get
            {
                if (!IsActive)
                {
                    return AuraStrength.None;
                }

                int _powerLevel = PowerLevel;
                if (_powerLevel < 2)
                {
                    return AuraStrength.Faint;
                }

                if (_powerLevel < 5)
                {
                    return AuraStrength.Moderate;
                }

                if (_powerLevel < 11)
                {
                    return AuraStrength.Strong;
                }

                return AuraStrength.Overwhelming;
            }
        }
        #endregion

        #region IAura Members
        public AuraStrength AuraStrength { get { return Strength; } }
        #endregion

        public override object Clone()
        {
            return new UndeadAuraAdjunct(Source);
        }
    }
}
