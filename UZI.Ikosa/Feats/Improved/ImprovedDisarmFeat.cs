using System;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Actions;
using System.Collections.Generic;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    AbilityRequirement(Abilities.MnemonicCode.Int, 13),
    FeatChainRequirement(typeof(ImprovedDefensiveCombatFeat)),
    FeatInfo(@"Improved Disarm")
    ]
    public class ImprovedDisarmFeat : FeatBase, IQualifyDelta
    {
        #region construction
        public ImprovedDisarmFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _Term = new TerminateController(this);
        }
        #endregion

        private readonly TerminateController _Term;

        public override string Benefit
        {
            get
            {
                return @"Will not provoke an opportunistic attack on disarm.  Opponent cannot disarm you on failure.  +4 to disarm opponent.";
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Creature.OpposedDeltable.Deltas.Add(this);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            DoTerminate();
        }

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if ((qualify is Interaction _iAct)
                && (_iAct.InteractData is MeleeAttackData)
                && (_iAct.InteractData as MeleeAttackData).Action is Disarm)
            {
                var _target = qualify.Target as IWeapon;
                if (qualify.Source as IWeaponHead != null)
                {
                    if ((((qualify.Source as IWeaponHead).ContainingWeapon.CreaturePossessor == Creature) && (qualify.Source as IWeaponHead).ContainingWeapon.IsActive)
                        || ((_target != null) && (_target.CreaturePossessor == Creature) && _target.IsActive))
                    {
                        yield return new QualifyingDelta(4, typeof(ImprovedDisarmFeat), @"Improved Disarm");
                    }
                }
            }
            yield break;
        }

        #endregion

        #region IControlTerminate Members

        public void DoTerminate()
        {
            if (!IsActive)
                _Term.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion
    }
}
