using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Shields;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class SunderStep : PreReqListStepBase
    {
        public SunderStep(CoreActivity activity, IAttackSource source)
            : base(activity)
        {
            _Source = source;
            var _opponent = Opponent;
            if (_opponent != null)
            {
                _PendingPreRequisites.Enqueue(
                    new RollPrerequisite(this, null, _opponent, @"Sunder.Opposed", @"Opposed Sunder",
                        new DieRoller(20), false));
            }
        }

        private IAttackSource _Source;
        public IAttackSource AttackSource { get { return _Source; } }

        public CoreActivity Activity { get { return Process as CoreActivity; } }

        public RollPrerequisite DefenderCheck
        {
            get
            {
                return AllPrerequisites<RollPrerequisite>(@"Sunder.Opposed").FirstOrDefault();
            }
        }

        #region public AttackTarget AttackTarget { get; }
        public AttackTarget AttackTarget
        {
            get
            {
                var _act = Activity;
                if (_act != null)
                {
                    return _act.Targets.OfType<AttackTarget>().FirstOrDefault();
                }
                return null;
            }
        }
        #endregion

        #region public ISlottedItem Item { get; }
        public ISlottedItem Item
        {
            get
            {
                var _atkTarget = AttackTarget;
                if (_atkTarget != null)
                {
                    return _atkTarget.Target as ISlottedItem;
                }
                return null;
            }
        }
        #endregion

        #region public Creature Opponent { get; }
        public Creature Opponent
        {
            get
            {
                var _item = Item;
                if (_item != null)
                {
                    return _item.CreaturePossessor;
                }
                return null;
            }
        }
        #endregion

        public AttackActionBase AttackAction
            => (Activity?.Action as ISupplyAttackAction)?.Attack;

        protected override bool OnDoStep()
        {
            if (AttackTarget?.Attack is MeleeAttackData _melee)
            {
                var _action = AttackAction;
                var _item = Item;
                if ((_melee.Attacker is Creature _attacker) 
                    && (_action != null) && (_item != null))
                {
                    var _check = Deltable.GetCheckNotify(_attacker.ID, $@"Sunder", _item.CreaturePossessor.ID, $@"Sunder Opposed");

                    // calculate effective opposed scores
                    var _opposed = _melee.ToOpposedAttackData();
                    var _atkInteract = new Interaction(Activity.Actor, AttackSource, Item, _opposed);
                    var _atkRoll = _opposed.AttackScore.QualifiedValue(_atkInteract, _check.CheckInfo);

                    var _defScore = new OpposedOpponentScore(DefenderCheck.RollValue, Opponent, _item, true);
                    var _defRoll = _defScore.Score.QualifiedValue(_atkInteract, _check.OpposedInfo);

                    // attack result step
                    _atkInteract.Feedback.Add(new AttackFeedback(this, _atkRoll > _defRoll));
                    AppendFollowing(Activity.GetActivityResultNotifyStep(
                        (_atkRoll > _defRoll) ? @"Success" : @"Fail"));
                    AppendFollowing(new AttackResultStep(Activity, null, _atkInteract, AttackSource));
                }
            }
            return true;
        }
    }
}
