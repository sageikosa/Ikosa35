using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class CounterDisarm : CoreStep
    {
        public CounterDisarm(CoreStep predecessor, Creature actor, Creature opponent, IMeleeWeapon source,
            IWeapon opponentWeapon)
            : base(predecessor)
        {
            _Actor = actor;
            _Opponent = opponent;
            _Source = source.MainHead;
            _OpponentWeapon = opponentWeapon;
            _Dispensing = true;
        }

        #region private data
        private bool _Dispensing;
        private Creature _Actor;
        private Creature _Opponent;
        private IWeaponHead _Source;
        private IWeapon _OpponentWeapon;
        #endregion

        public override bool IsDispensingPrerequisites => _Dispensing;

        public ChoicePrerequisite CounterAttemptChoice
            => AllPrerequisites<ChoicePrerequisite>(@"CounterDisarm.Attempt").FirstOrDefault();

        public RollPrerequisite AttackerCheck
            => AllPrerequisites<RollPrerequisite>(@"CounterDisarm.AttackRoll").FirstOrDefault();

        public RollPrerequisite DefenderCheck
            => AllPrerequisites<RollPrerequisite>(@"CounterDisarm.Opposed").FirstOrDefault();

        #region protected override StepPrerequisite OnNextPrerequisite()
        protected override StepPrerequisite OnNextPrerequisite()
        {
            if (!IsDispensingPrerequisites)
                return null;

            var _willCounter = CounterAttemptChoice;
            if (_willCounter == null)
            {
                // actor must make decision to counter-disarm [serial]
                return new ChoicePrerequisite(this, _Actor, this, _Opponent, @"CounterDisarm.Attempt", @"Attempt a counter disarm",
                    DecideToCounter(), true);
            }

            // if decision was "Yes", continue
            if ((_willCounter.Selected as OptionAimValue<bool>)?.Value ?? false)
            {
                // make the attack roll as a pre-requisite
                if (AttackerCheck == null)
                    return new RollPrerequisite(this, null, _Actor, @"CounterDisarm.AttackRoll", @"Counter Disarm Attack",
                        new DieRoller(20), false);
                if (DefenderCheck == null)
                    return new RollPrerequisite(this, null, _Opponent, @"CounterDisarm.Opposed", @"Opposed Disarm",
                        new DieRoller(20), false);
            }

            // done
            _Dispensing = false;
            return null;
        }
        #endregion

        #region private IEnumerable<OptionAimOption> DecideToCounter()
        private IEnumerable<OptionAimOption> DecideToCounter()
        {
            yield return new OptionAimValue<bool>
            {
                Key = @"True",
                Description = @"Attempt disarm",
                Name = @"Yes",
                Value = true
            };
            yield return new OptionAimValue<bool>
            {
                Key = @"False",
                Description = @"Do not attempt disarm",
                Name = @"No",
                Value = false
            };
            yield break;
        }
        #endregion

        protected override bool OnDoStep()
        {
            var _willCounter = CounterAttemptChoice;
            if (_willCounter != null)
            {
                if (ManipulateTouch.CanManipulateTouch(_Actor, _OpponentWeapon))
                {
                    if ((_willCounter.Selected is OptionAimValue<bool> _doCounter) && _doCounter.Value)
                    {
                        // counter disarm chosen
                        var _opposed = new OpposedAttackData(_Actor, new Disarm(_Source, @"201"), _Actor.GetLocated()?.Locator, AttackImpact.Penetrating,
                             new Deltable(Math.Min(AttackerCheck.RollValue, 20)), false, null, null, 0, 1);
                        var _atkInteract = new Interaction(_Actor, _Source, _OpponentWeapon, _opposed);

                        var _check = Deltable.GetCheckNotify(_Actor.ID, @"Counter Disarm", _Opponent.ID, @"Counter Disarm Opposed");
                        var _atkRoll = _opposed.AttackScore.QualifiedValue(_atkInteract, _check.CheckInfo);

                        var _defScore = new OpposedOpponentScore(DefenderCheck.RollValue, _Opponent, _OpponentWeapon, false);
                        var _defRoll = _defScore.Score.QualifiedValue(_atkInteract, _check.OpposedInfo);

                        if (_atkRoll > _defRoll)
                        {
                            var _purloin = new Purloin(_Actor, new ActionTime(TimeType.Brief));
                            var _workSet = new Interaction(_Actor, _Source, _OpponentWeapon, _purloin);
                            _OpponentWeapon.HandleInteraction(_workSet);
                        }
                    }
                }
            }
            return true;
        }
    }
}
