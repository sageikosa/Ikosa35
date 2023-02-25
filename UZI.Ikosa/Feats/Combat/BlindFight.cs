using System;
using Uzi.Core;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Feats
{
    [Serializable, FighterBonusFeat, FeatInfo("Blind-Fight")]
    public class BlindFight : FeatBase, IMonitorChange<InteractionAlteration>
    {
        public BlindFight(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Creature?.AddChangeMonitor(this);
        }

        protected override void OnDeactivate()
        {
            Creature?.RemoveChangeMonitor(this);
            base.OnDeactivate();
        }

        public override string Benefit
            => @"Reroll concealment miss chance.  No penalties versus invisible melee attacker.  3/4 speed when unable to see.";

        // IMonitorChange<InteractionAlteration> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<InteractionAlteration> args)
        {
            // block max dexterity alterations due to awareness loss
            if (args.Action.Equals(AlterationSet.TargetAction))
            {
                if (args.NewValue is TargetUnawareAlteration)
                {
                    var _atk = (args.NewValue as TargetUnawareAlteration).InteractData as AttackData;
                    if (!(_atk is RangedAttackData))
                        args.DoAbort(@"Blind-Fight", this);
                }
                else if ((args.NewValue as MaxDexterityToARAlteration)?.UnawarenessLoss ?? false)
                {
                    var _atk = (args.NewValue as MaxDexterityToARAlteration).InteractData as AttackData;
                    if (!(_atk is RangedAttackData))
                        args.DoAbort(@"Blind-Fight", this);
                }
            }
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<InteractionAlteration> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<InteractionAlteration> args)
        {
        }
    }
}
