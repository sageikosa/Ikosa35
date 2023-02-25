using System;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Improved Spell Penetration", true),
    FeatChainRequirement(typeof(SpellPenetrationFeat))
    ]
    public class ImprovedSpellPenetrationFeat : FeatBase, IQualifyDelta
    {
        public ImprovedSpellPenetrationFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _TermCtrl = new TerminateController(this);
            _Delta = new QualifyingDelta(2, typeof(ImprovedSpellPenetrationFeat), @"Improved Spell Penetration");
        }

        #region data
        private IDelta _Delta;
        private readonly TerminateController _TermCtrl;
        #endregion

        public override string Benefit
            => @"+2 power level check against Spell Resistant targets (improves spell penetration)";

        protected override void OnActivate()
        {
            base.OnActivate();
            if (Creature != null)
            {
                Creature.ExtraClassPowerLevel.Deltas.Add(this);
            }
        }

        protected override void OnDeactivate()
        {
            DoTerminate();
            base.OnDeactivate();
        }

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => (IsActive && (qualify as Interaction)?.InteractData is SpellResistanceCheckData
            ? _Delta
            : null).ToEnumerable().Where(_d => _d != null);

        #region IControlTerminate Members

        public void DoTerminate()
        {
            _TermCtrl.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _TermCtrl.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _TermCtrl.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _TermCtrl.TerminateSubscriberCount;

        #endregion
    }
}