using System;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using System.Collections.Generic;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FighterBonusFeat,
    FeatInfo(@"Point Blank Shot")
    ]
    public class PointBlankShotFeat : FeatBase, IQualifyDelta
    {
        #region construction
        public PointBlankShotFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _Term = new TerminateController(this);
            _Delta = new QualifyingDelta(1, typeof(PointBlankShotFeat), @"Point Blank Shot");
        }
        #endregion

        #region data
        private TerminateController _Term;
        private IDelta _Delta;
        #endregion

        protected override void OnActivate()
        {
            base.OnActivate();
            Creature.RangedDeltable.Deltas.Add(this);
            Creature.ExtraWeaponDamage.Deltas.Add(this);
        }

        protected override void OnDeactivate()
        {
            // terminates qualify deltas
            DoTerminate();
            base.OnDeactivate();
        }

        public override string Benefit => @"+1 Attack and Damage with ranged weapons within 30 feet.";

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // make sure its a ranged attack
            if (qualify is Interaction)
            {
                var _workSet = qualify as Interaction;
                if (_workSet.InteractData is RangedAttackData _atk)
                {
                    // find target, make sure it's in range
                    if (_workSet.Target is ICoreObject _target)
                    {
                        var _trgLoc = _target.GetLocated().Locator;
                        var _distance = _trgLoc.GeometricRegion.NearDistance(_atk.AttackPoint);
                        if (_distance <= 30d)
                        {
                            yield return _Delta;
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
