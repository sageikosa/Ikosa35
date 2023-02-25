using System;
using Uzi.Ikosa.Advancement;
using Uzi.Core;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    FeatChainRequirement(typeof(PointBlankShotFeat)),
    FeatInfo("Precise Shot")
    ]
    public class PreciseShotFeat : FeatBase, IMonitorChange<InteractionAlteration>
    {
        public PreciseShotFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override void OnActivate()
        {
            Creature.AddChangeMonitor((IMonitorChange<InteractionAlteration>)this);
        }

        protected override void OnDeactivate()
        {
            Creature.RemoveChangeMonitor((IMonitorChange<InteractionAlteration>)this);
        }

        public override string Benefit
            => @"Ranged attack into melee without -4 penalty.";

        #region IMonitorChange<InteractionAlteration> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<InteractionAlteration> args)
        {
            if (args.Action.Equals(AlterationSet.ActorAction))
            {
                // TODO: suppress ranged attack into melee penalty
            }
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<InteractionAlteration> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<InteractionAlteration> args)
        {
        }
        #endregion
    }
}
