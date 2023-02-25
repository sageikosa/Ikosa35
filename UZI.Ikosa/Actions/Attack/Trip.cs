using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Feats;
using Uzi.Core;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Core.Dice;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class Trip : AttackActionBase, IAttackSource, IRangedSourceProvider
    {
        #region ctor()
        public Trip(IWeaponHead source, string orderKey)
            : base(source, false, true, orderKey)
        {
            var _wpn = WeaponHead.ContainingWeapon;
            Improved = _wpn.CreaturePossessor.Feats.Contains(typeof(ImprovedTripFeat));
            _Target = !(Improved
                || _wpn.GetWieldTemplate().NotIn(WieldTemplate.Unarmed, WieldTemplate.TooSmall, WieldTemplate.TooBig));
        }
        #endregion

        #region public override int StandardAttackBonus { get; }
        public override int StandardAttackBonus
            => WeaponHead.AttackBonus.EffectiveValue + Weapon.CreaturePossessor.MeleeDeltable.QualifiedValue(
                new Interaction(Weapon.CreaturePossessor, WeaponHead, null,
                    new MeleeAttackData(null, this, Weapon.GetLocated()?.Locator, AttackImpact.Touch, null, false, null, null, 1, 1)));
        #endregion

        public IMeleeWeapon MeleeWeapon => Weapon as IMeleeWeapon;

        /// <summary>Use the action to start the trip workflow</summary>
        public override IAttackSource AttackSource => this;

        public bool Improved { get; private set; }
        public override string Key => @"Attack.Trip";
        public override string DisplayName(CoreActor actor) => $@"Trip with {MeleeWeapon.GetKnownName(actor)}";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Trip", activity.Actor, observer,
                activity.GetFirstTarget<AttackTarget>(@"Trip.Target")?.Target as CoreObject);
            _obs.Implement = GetInfoData.GetInfoFeedback(WeaponHead, observer);
            return _obs;
        }

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            // target creatures in melee range
            yield return new AttackAim(@"Trip.Target", @"Trip Target", AttackImpact.Touch,
                Lethality.AlwaysNonLethal, false, WeaponHead.CriticalLow, null, FixedRange.One, FixedRange.One,
                new StrikeZoneRange(MeleeWeapon), new CreatureTargetType());
            yield break;
        }
        #endregion

        #region IAttackSource Members

        public DeltableQualifiedDelta AttackBonus
            => WeaponHead.AttackBonus;

        public int CriticalRange
            => WeaponHead.CriticalRange;

        public DeltableQualifiedDelta CriticalRangeFactor
            => WeaponHead.CriticalRangeFactor;

        public DeltableQualifiedDelta CriticalDamageFactor
            => WeaponHead.CriticalDamageFactor;

        #region public IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction workSet)
        /// <summary>Queue up prerequisites (opposed trip rolls)</summary>
        public IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction attack)
        {
            if (attack != null)
            {
                var _feedback = attack.Feedback.OfType<AttackFeedback>().FirstOrDefault();
                if ((_feedback != null) && _feedback.Success)
                {
                    yield return new RollPrerequisite(this, attack, attack.Actor, @"Attacker",
                        @"Strength to Trip", new DieRoller(20), false);
                    yield return new RollPrerequisite(this, attack, attack.Target as CoreActor, @"Defender",
                        @"Resist Trip", new DieRoller(20), false);
                }
            }
            yield break;
        }
        #endregion

        #region public void AttackResult(AttackResultStep result, Interaction workSet)
        public void AttackResult(AttackResultStep result, Interaction workSet)
        {
            if (workSet != null)
            {
                var _feedback = workSet.Feedback.OfType<AttackFeedback>().FirstOrDefault();
                if ((_feedback != null) && _feedback.Success)
                {
                    var _atkRoll = result.AllPrerequisites<RollPrerequisite>(@"Attacker").FirstOrDefault();
                    var _defRoll = result.AllPrerequisites<RollPrerequisite>(@"Defender").FirstOrDefault();
                    if ((_atkRoll != null) && (_defRoll != null))
                    {
                        DoTrip(result, _atkRoll, _defRoll, true, MeleeWeapon as ITrippingWeapon);
                        return;
                    }
                }
            }
        }
        #endregion

        public bool IsSourceChannel(IAttackSource source)
            => (source == this)
            || (source == WeaponHead);

        #endregion

        #region public static void DoTrip(CoreStep result, RollPrerequisite atkRoll, RollPrerequisite defRoll, bool canCounter)
        /// <summary>Can use this outside the Trip attack</summary>
        public static void DoTrip(CoreStep result, RollPrerequisite atkRoll, RollPrerequisite defRoll, bool canCounter, ITrippingWeapon weapon)
        {
            // trip qualifier
            var _tripper = atkRoll.Fulfiller as Creature;
            var _target = defRoll.Fulfiller as Creature;
            var _atkQual = new Qualifier(_tripper, typeof(Trip), _target);

            var _check = Deltable.GetCheckNotify(_tripper?.ID, @"Trip", _target.ID, @"Trip Defend");
            while (true)
            {
                // attacker result
                _check.CheckInfo.Deltas.Clear();
                var _atkCheck = (_tripper.Abilities.Strength.CheckValue(_atkQual, atkRoll.RollValue, _check.CheckInfo) ?? 0);
                var _tripperOM = _tripper.BodyDock.OpposedModifier.Value;
                if (_tripperOM != 0)
                {
                    _atkCheck += _tripperOM;
                    _check.CheckInfo.AddDelta(_tripper.BodyDock.OpposedModifier.Name, _tripperOM);
                    _check.CheckInfo.Result += _tripperOM;
                }

                // defender result
                var _strCheck = new DeltaCalcInfo(_target.ID, @"Trip Defend");
                var _dexCheck = new DeltaCalcInfo(_target.ID, @"Trip Defend");
                var _defCheck = Math.Max(
                    _target.Abilities.Strength.CheckValue(_atkQual, defRoll.RollValue, _strCheck) ?? 0,
                    _target.Abilities.Dexterity.CheckValue(_atkQual, defRoll.RollValue, _dexCheck) ?? 0);
                _check.OpposedInfo = (_dexCheck.Result > _strCheck.Result)
                    ? _dexCheck
                    : _strCheck;
                var _targetOM = _target.BodyDock.OpposedModifier.Value;
                if (_targetOM != 0)
                {
                    _check.OpposedInfo.AddDelta(_target.BodyDock.OpposedModifier.Name, _targetOM);
                    _check.OpposedInfo.Result += _targetOM;
                }

                // if defender is helpless, this always succeeds
                if ((_atkCheck > _check.OpposedInfo.Result) || _target.Conditions.Contains(Condition.Helpless))
                {
                    // attacker wins
                    var _prone = new ProneEffect(typeof(Trip));
                    _target.AddAdjunct(_prone);
                    result.EnqueueNotify(new RefreshNotify(false, true, false, false, false), _target.ID, _tripper.ID);
                    return;
                }
                else if (_atkCheck < _defCheck)
                {
                    // defender wins
                    if (canCounter)
                    {
                        result.AppendFollowing(new CounterTrip(result, _target, _tripper, weapon));
                    }
                    return;
                }

                // dreaded tie...rather than going back to players, just do a run-off
                atkRoll.RollValue = atkRoll.Roller.RollValue(_tripper.ID, @"Trip Tie", @"Attacker", _tripper.ID);
                defRoll.RollValue = defRoll.Roller.RollValue(_target.ID, @"Trip Tie", @"Defender", _target.ID);
            }
        }
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => IsAttackProvocableTarget(activity, potentialTarget, @"Trip.Target");

        public override void AttackResultEffects(AttackResultStep result, Interaction workSet)
        {
        }

        // IAdjunctSet Members
        public AdjunctSet Adjuncts
            => WeaponHead.Adjuncts;

        // IRangedSourceProvider Members
        public IRangedSource GetRangedSource(CoreActor actor, ActionBase action, RangedAim aim, IInteract target)
            => Weapon as IRangedSource;
    }
}