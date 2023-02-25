using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Feats
{
    [
        Serializable,
        FeatInfo(@"Dodge", true),
        AbilityRequirement(MnemonicCode.Dex, 13),
        FighterBonusFeat
    ]
    public class DodgeFeat : FeatBase, IQualifyDelta, IActionProvider, IActionSource
    {
        public DodgeFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _TermCtrl = new TerminateController(this);
            _Delta = new QualifyingDelta(1, typeof(DodgeFeat), @"Dodge");
            _TargetID = null;
        }

        #region data
        private IDelta _Delta;
        private readonly TerminateController _TermCtrl;
        private Guid? _TargetID;
        #endregion

        // TODO: mechanism to express as awareness...
        public Guid? TargetID { get => _TargetID; set => _TargetID = value; }

        public override string Benefit => @"+1 Dodge to Armor Rating versus designated opponent";

        public Guid ID => CoreID;

        #region OnActivate
        protected override void OnActivate()
        {
            base.OnActivate();
            Creature?.Actions.Providers.Add(this, this);
            Creature?.IncorporealArmorRating.Deltas.Add(this);
            Creature?.TouchArmorRating.Deltas.Add(this);
            Creature?.NormalArmorRating.Deltas.Add(this);
        }
        #endregion

        #region OnDeactivate
        protected override void OnDeactivate()
        {
            DoTerminate();
            Creature?.Actions.Providers.Remove(this);
            base.OnDeactivate();
        }
        #endregion

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => (IsActive
            && (qualify is Interaction _iAct)
            && (_iAct.InteractData is AttackData _atk)
            && (_atk.Attacker.ID == _TargetID)
            && (Creature?.CanDodge(_iAct) ?? false)
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

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if ((budget is LocalActionBudget _budget)
                && _budget.IsInitiative
                && (_budget.TurnTick?.TurnTracker.FocusedBudget == _budget)
                && _budget.CanPerformBrief)
            {
                // choice can only be made during combat, while something can still be done
                yield return new DodgeChoice(this);
            }
            yield break;
        }

        // TODO: mechanism to express as awareness (wrap or extend FeatInfo?)...
        public Info GetProviderInfo(CoreActionBudget budget)
            => ToFeatInfo();
        #endregion

        // IActionSource
        public IVolatileValue ActionClassLevel
            => Creature?.ActionClassLevel;
    }

    public class DodgeChoice : ActionBase
    {
        internal DodgeChoice(DodgeFeat source)
            : base(source, new ActionTime(Contracts.TimeType.FreeOnTurn), false, false, @"200")
        {
        }

        public override string Key => @"Dodge.Select";
        public override string DisplayName(CoreActor actor) => @"Select Dodge Target";
        public DodgeFeat DodgeFeat => Source as DodgeFeat;
        public override bool IsMental => true;
        public override bool IsChoice => true;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        private IEnumerable<Creature> DodgeTargets()
            => DodgeFeat.Creature.Awarenesses.UnFriendlyAwarenesses
            .Union(DodgeFeat.Creature.Awarenesses.FriendlyAwarenesses)
            .ToList();

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new ObjectListAim(@"Target", @"Dodge Target", FixedRange.One, FixedRange.One, DodgeTargets());
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (activity.Targets[0].Target is Creature _target)
            {
                // set target
                DodgeFeat.TargetID = _target.ID;

                // TODO: register choice with budget...?

                // report change
                var _info = GetInfoData.GetInfoFeedback(_target, DodgeFeat.Creature);
                if (_info != null)
                {
                    // status step (using info)
                    return new NotifyStep(activity, new SysNotify(@"Choice",
                        new Description(@"Dodge", $@"Set to {_info.Message}"),
                        _info))
                    {
                        InfoReceivers = new Guid[] { activity.Actor.ID }
                    };
                }

                // status step (using ID)
                return new NotifyStep(activity, new SysNotify(@"Choice",
                    new Description(@"Dodge", $@"Set to {_target.ID}")))
                {
                    InfoReceivers = new Guid[] { activity.Actor.ID }
                };
            }

            // status step
            return new NotifyStep(activity, new SysNotify(@"Choice",
            new Description(@"Dodge", @"Bad selection")))
            {
                InfoReceivers = new Guid[] { activity.Actor.ID }
            };
        }
    }
}
