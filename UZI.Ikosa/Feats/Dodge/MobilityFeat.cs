using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Feats
{
    [
        Serializable,
        FeatInfo(@"Mobility", true),
        AbilityRequirement(MnemonicCode.Dex, 13),
        FeatChainRequirement(typeof(DodgeFeat)),
        FighterBonusFeat
    ]
    public class MobilityFeat : FeatBase, IQualifyDelta
    {
        public MobilityFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _TermCtrl = new TerminateController(this);
            _Delta = new QualifyingDelta(4, typeof(MobilityFeat), @"Mobility");
        }

        #region data
        private IDelta _Delta;
        private readonly TerminateController _TermCtrl;
        #endregion

        public override string Benefit => @"+4 Dodge to Armor Rating versus movement opportunistic attacks";

        #region OnActivate
        protected override void OnActivate()
        {
            base.OnActivate();
            Creature?.NormalArmorRating.Deltas.Add(this);
            Creature?.IncorporealArmorRating.Deltas.Add(this);
            Creature?.TouchArmorRating.Deltas.Add(this);
        }
        #endregion

        #region OnDeactivate
        protected override void OnDeactivate()
        {
            DoTerminate();
            base.OnDeactivate();
        }
        #endregion

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => (IsActive
            && (qualify is StepInteraction _step)                   // get step workflow information
            && (_step.Step is AttackStep _atkStep)                  // validate an attack
            && (_atkStep.TargetProcess is CoreActivity _activity)   // get the activity for the attack
            && (_activity.Action is OpportunisticAttack _oppAtk)    // ensure it's an opportunity
            && (_oppAtk.Opportunity.Action is MovementAction)       // ensure the trigger was movement
            && (Creature?.CanDodge(_step) ?? false)                 // make sure dodge will work
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
