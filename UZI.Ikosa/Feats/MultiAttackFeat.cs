using System;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using System.Collections.Generic;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Multi-Attack", true),
    NaturalWeaponRequirement(3)
    ]
    public class MultiAttackFeat : FeatBase, IQualifyDelta
    {
        public MultiAttackFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _Term = new TerminateController(this);
        }

        protected override void OnActivate()
        {
            Creature.MeleeDeltable.Deltas.Add(this);
            Creature.RangedDeltable.Deltas.Add(this);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            // terminate deltas
            DoTerminate();
            base.OnDeactivate();
        }

        private readonly TerminateController _Term;

        public override string Benefit { get { return @"Secondary attacks with natural weapons take only a -2 penalty."; } }
        public override string PreRequisite { get { return @"3 or more natural attacks."; } }

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (!(qualify is Interaction _iAct))
            {
                yield break;
            }

            if ((_iAct.InteractData is AttackData) && (qualify.Source is IWeaponHead _wpnHead))
            {
                if ((_wpnHead.ContainingWeapon is NaturalWeapon _natrl) && !_natrl.IsPrimary)
                {
                    yield return new QualifyingDelta(3, typeof(MultiAttackFeat), @"Multi-Attack");
                }
            }
            yield break;
        }

        #endregion

        #region IControlTerminate Members

        public void DoTerminate()
        {
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
