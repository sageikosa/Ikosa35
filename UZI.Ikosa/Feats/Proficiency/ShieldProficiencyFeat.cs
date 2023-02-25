using System;
using Uzi.Ikosa.Items.Shields;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
        // proficiency check requirement
    FeatInfo(@"Shield Proficiency")
    ]
    public class ShieldProficiencyFeat: FeatBase, IShieldProficiency
    {
        public ShieldProficiencyFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return @"You can use a shield and take only the standard penalties."; }
        }

        public override bool MeetsPreRequisite(Creature creature)
        {
            // if not proficient with a standard shield, then this feat can be taken
            return !creature.Proficiencies.IsProficientWithShield(false, PowerLevel);
        }

        protected override void OnAdd()
        {
            base.OnAdd();
            Creature.Proficiencies.Add(this);
        }

        protected override void OnRemove()
        {
            Creature.Proficiencies.Remove(this);
            base.OnRemove();
        }

        #region IProficiencyCheck Members
        public bool IsProficientWithShield(bool tower, int powerLevel)
        {
            return !tower && (powerLevel >= PowerLevel);
        }

        public bool IsProficientWith(ShieldBase shield, int powerLevel)
        {
            return !shield.Tower && (powerLevel >= PowerLevel);
        }

        public string Description
        {
            get { return @"Shields"; }
        }
        #endregion
    }
}
