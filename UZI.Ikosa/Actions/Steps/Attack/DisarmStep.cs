using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class DisarmStep : PreReqListStepBase
    {
        public DisarmStep(CoreActivity activity, IAttackSource source, bool noCounter)
            : base(activity)
        {
            _Source = source;
            _NoCounter = noCounter;
            var _opponent = Opponent;
            if (_opponent != null)
            {
                _PendingPreRequisites.Enqueue(
                    new RollPrerequisite(this, null, _opponent, @"Disarm.Opposed", @"Opposed Disarm",
                        new DieRoller(20), false));
            }
        }

        #region private data
        private bool _NoCounter;
        private IAttackSource _Source;
        #endregion

        public IAttackSource AttackSource => _Source;
        public bool NoCounter => _NoCounter;
        public CoreActivity Activity => Process as CoreActivity;

        public RollPrerequisite DefenderCheck
            => AllPrerequisites<RollPrerequisite>(@"Disarm.Opposed").FirstOrDefault();

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

        #region public ICoreObject TargetObject { get; }
        public ICoreObject TargetObject
        {
            get
            {
                var _atkTarget = AttackTarget;
                if (_atkTarget != null)
                {
                    return _atkTarget.Target as ICoreObject;
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
                var _item = TargetObject;
                if (_item != null)
                {
                    if (_item is ISlottedItem)
                    {
                        // item is slotted
                        return (_item as ISlottedItem).CreaturePossessor;
                    }
                    else if (_item is IAdjunctable)
                    {
                        var _adj = _item as IAdjunctable;
                        var _held = _adj.Adjuncts.OfType<Held>().FirstOrDefault();
                        if (_held != null)
                        {
                            // holding wrapper for item
                            return _held.HoldingWrapper.CreaturePossessor;
                        }
                        else
                        {
                            // attended objects...
                            var _att = _adj.Adjuncts.OfType<Attended>().FirstOrDefault();
                            if (_att != null)
                            {
                                return _att.Creature;
                            }
                        }
                    }
                }
                return null;
            }
        }
        #endregion

        public AttackActionBase AttackAction
            => (Activity?.Action as ISupplyAttackAction)?.Attack;

        protected override bool OnDoStep()
        {
            var _atkTarget = AttackTarget;
            if ((_atkTarget != null) && (_atkTarget.Attack is MeleeAttackData))
            {
                var _action = AttackAction;
                var _opponent = Opponent;
                var _obj = TargetObject;
                if ((_atkTarget.Attack.Attacker is Creature _attacker)
                    && (_action != null)
                    && (_obj != null)
                    && (_opponent != null))
                {
                    if (!ManipulateTouch.CanManipulateTouch(_attacker, _obj))
                    {
                        EnqueueNotify(new BadNewsNotify(_attacker.ID, @"Disarm", new Description(@"Cannot interact")));
                    }
                    else
                    {
                        // get first empty hand
                        var _emptyHand = _attacker.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
                            .FirstOrDefault(_s => _s.SlottedItem == null);

                        // calculate effective opposed scores
                        var _opposed = (_atkTarget.Attack as MeleeAttackData).ToOpposedAttackData();
                        var _atkInteract = new Interaction(Activity.Actor, AttackSource, _obj, _opposed);

                        var _check = Deltable.GetCheckNotify(_attacker.ID, @"Disarm", _opponent.ID, @"Disarm Opposed");
                        var _atkRoll = _opposed.AttackScore.QualifiedValue(_atkInteract, _check.CheckInfo);

                        var _defScore = new OpposedOpponentScore(DefenderCheck.RollValue, Opponent, _obj, false);
                        var _defRoll = _defScore.Score.QualifiedValue(_atkInteract, _check.OpposedInfo);

                        if (_atkRoll > _defRoll)
                        {
                            var _melee = _action.Weapon as IMeleeWeapon;
                            if ((_melee != null)
                                && (_melee.WieldTemplate == WieldTemplate.Unarmed)
                                && (_emptyHand != null))
                            {
                                var _purloin = new Purloin(_attacker, AttackAction.TimeCost);
                                var _workSet = new Interaction(_attacker, _emptyHand, _obj, _purloin);
                                _obj.HandleInteraction(_workSet);
                            }
                            else
                            {
                                var _purloin = new Purloin(_attacker, AttackAction.TimeCost);
                                var _workSet = new Interaction(_attacker, _melee, _obj, _purloin);
                                _obj.HandleInteraction(_workSet);
                            }
                        }
                        else if (!NoCounter && (TargetObject is IMeleeWeapon) && (AttackSource is IWeaponHead))
                        {
                            // NOTE: only allowing a counter with a melee weapon
                            AppendFollowing(new CounterDisarm(this, _opponent, _attacker, TargetObject as IMeleeWeapon, (AttackSource as IWeaponHead).ContainingWeapon));
                        }
                    }
                }
            }
            return true;
        }
    }
}
