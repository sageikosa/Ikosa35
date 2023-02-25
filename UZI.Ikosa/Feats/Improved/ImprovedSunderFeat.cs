using System;
using System.Linq;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Adjuncts;
using System.Collections.Generic;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    AbilityRequirement(Abilities.MnemonicCode.Str, 13),
    FeatChainRequirement(typeof(PowerAttackFeat)),
    FeatInfo(@"Improved Sunder")
    ]
    public class ImprovedSunderFeat : FeatBase, IQualifyDelta
    {
        #region construction
        public ImprovedSunderFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _Term = new TerminateController(this);
        }
        #endregion

        private readonly TerminateController _Term;

        public override string Benefit => @"Will not provoke an opportunistic attack on sunder.  +4 ATK on sunder.";

        protected override void OnActivate()
        {
            base.OnActivate();

            // +4 to any sunder
            Creature.OpposedDeltable.Deltas.Add(this);
            Creature.MeleeDeltable.Deltas.Add(this);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            DoTerminate();
        }

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // must be a sunder
            if ((qualify is Interaction _iAct)
                && (_iAct.InteractData is MeleeAttackData)
                && (_iAct.InteractData as MeleeAttackData).Action is SunderWieldedItem)
            {
                // must be with a weapon possessed by this creature
                // therefore, this feat only works for the attacker
                if (qualify.Source is IWeaponHead _head)
                {
                    if (((_head.ContainingWeapon.CreaturePossessor == Creature)
                        && _head.ContainingWeapon.IsActive))
                    {
                        // target item must be attended
                        if (qualify.Target is IAdjunctable _target)
                        {
                            if (_target.HasAdjunct<Attended>())
                                yield return new QualifyingDelta(4, typeof(ImprovedSunderFeat), @"Improved Sunder");
                        }
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