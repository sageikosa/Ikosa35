using System;
using System.Linq;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Core;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class FighterLevelRequirementAttribute : RequirementAttribute
    {
        public FighterLevelRequirementAttribute(int minVal)
        {
            MinimumValue = minVal;
        }
        public readonly int MinimumValue;
        public override string Name { get { return string.Format(@"Fighter Level >= {0}", MinimumValue); } }
        public override string Description { get { return string.Format(@"Fighter Level must be {0} or higher at the designated power die.", MinimumValue); } }

        public override bool MeetsRequirement(Creature creature)
        {
            return this.MeetsRequirement(creature, creature.AdvancementLog.NumberPowerDice);
        }

        public override bool MeetsRequirement(Creature creature, int powerLevel)
        {
            // must have a fighter class
            var _fighter = creature.Classes.OfType<Fighter>().FirstOrDefault();
            if (_fighter != null)
            {
                var _ftrLevel = _fighter.EffectiveLevel.QualifiedValue(PowerLevelCheck.LevelCheck(powerLevel));
                return (_ftrLevel >= MinimumValue);
            }

            // no fighter levels or does not have the fighter level at the power level
            return false;
        }

        public override RequirementMonitor CreateMonitor(IRefreshable target, Creature owner)
        {
            return new PowerDiceMonitor<FighterLevelRequirementAttribute>(this, target, owner);
        }
    }
}
