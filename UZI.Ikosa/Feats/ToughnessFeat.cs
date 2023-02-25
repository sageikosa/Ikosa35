using System;

namespace Uzi.Ikosa.Feats
{
    /// <summary>
    /// Add 3 hit-points to the creature
    /// </summary>
    [
    Serializable,
    FeatInfo("Toughness", false)
    ]
    public class ToughnessFeat: FeatBase
    {
        /// <summary>
        /// Add 3 extra hit-points to the creature
        /// </summary>
        public ToughnessFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return "You gain +3 hit points."; }
        }

        protected override void OnAdd()
        {
            base.OnAdd();
            this.Creature.HealthPoints.ExtraHealthPoints += 3;
        }

        protected override void OnRemove()
        {
            this.Creature.HealthPoints.ExtraHealthPoints -= 3;
            base.OnRemove();
        }
    }
}
