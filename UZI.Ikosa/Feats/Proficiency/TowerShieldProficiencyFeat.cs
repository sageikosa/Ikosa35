using System;
using Uzi.Ikosa.Items.Shields;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
        // shield proficiency check
    FeatInfo(@"Tower Shield Proficiency")
    ]
    public class TowerShieldProficiencyFeat: FeatBase, IShieldProficiency
    {
        public TowerShieldProficiencyFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return @"You can use a tower shield and suffer only the standard penalties."; }
        }

        public override bool MeetsPreRequisite(Creature creature)
        {
            if (IgnorePreRequisite)
            {
                return true;
            }

            // proficient with shields, but not tower shields
            return (creature.Proficiencies.IsProficientWithShield(false, PowerLevel)
                && (!creature.Proficiencies.IsProficientWithShield(true, PowerLevel)));
        }

        public override string PreRequisite { get { return @"Shield Proficiency"; } }

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

        #region IShieldProficiency Members
        public bool IsProficientWithShield(bool tower, int powerLevel)
        {
            return (powerLevel >= PowerLevel);
        }

        public bool IsProficientWith(ShieldBase shield, int powerLevel)
        {
            return (powerLevel >= PowerLevel);
        }

        public string Description
        {
            get { return @"Tower shields"; }
        }
        #endregion
    }
}
