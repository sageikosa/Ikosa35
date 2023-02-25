using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class CounterTrip : CoreStep
    {
        public CounterTrip(CoreStep predecessor, Creature tripper, Creature target, ITrippingWeapon opponentWeapon)
            : base(predecessor)
        {
            _Tripper = tripper;
            _Target = target;
            _OpponentWeapon = opponentWeapon;
            _Dispensing = true;
        }

        #region private data
        private bool _Dispensing;
        private Creature _Tripper;
        private Creature _Target;
        private ITrippingWeapon _OpponentWeapon;
        #endregion

        public override bool IsDispensingPrerequisites => _Dispensing;

        public ChoicePrerequisite CounterAttemptChoice
            => AllPrerequisites<ChoicePrerequisite>(@"CounterTrip.Attempt").FirstOrDefault();

        public ChoicePrerequisite DropWeaponChoice
            => AllPrerequisites<ChoicePrerequisite>(@"CounterTrip.DropWeapon").FirstOrDefault();

        #region protected override StepPrerequisite OnNextPrerequisite()
        protected override StepPrerequisite OnNextPrerequisite()
        {
            if (!IsDispensingPrerequisites)
                return null;

            var _willCounter = CounterAttemptChoice;
            if (_willCounter == null)
            {
                // defender must make decision to counter-trip [serial]
                return new ChoicePrerequisite(this, _Tripper, this, _Target, @"CounterTrip.Attempt", @"Attempt a counter trip",
                    DecideToCounter(), true);
            }

            // if decision was "Yes", continue
            var _doCounter = _willCounter.Selected as OptionAimValue<bool>;
            if ((_doCounter != null) && _doCounter.Value)
            {
                // opponent drop (if possible)
                if ((_OpponentWeapon != null) && _OpponentWeapon.AvoidCounterByDrop)
                {
                    var _willDrop = DropWeaponChoice;
                    if (_willDrop == null)
                    {
                        // attacker must make decision to drop weapon [serial]
                        return new ChoicePrerequisite(this, _Target, this, _Tripper, @"CounterTrip.DropWeapon", @"Avoid counter trip by dropping weapon",
                            DecideToDrop(), true);
                    }

                    var _doDrop = _willDrop.Selected as OptionAimValue<bool>;
                    if ((_doDrop != null) && _doDrop.Value)
                    {
                        // done
                        _Dispensing = false;
                        return null;
                    }
                }
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
                Description = @"Attempt trip",
                Name = @"Yes",
                Value = true
            };
            yield return new OptionAimValue<bool>
            {
                Key = @"False",
                Description = @"Do not attempt trip",
                Name = @"No",
                Value = false
            };
            yield break;
        }
        #endregion

        #region private IEnumerable<OptionAimOption> DecideToDrop()
        private IEnumerable<OptionAimOption> DecideToDrop()
        {
            yield return new OptionAimValue<bool>
            {
                Key = @"True",
                Description = @"Drop weapon",
                Name = @"Yes",
                Value = true
            };
            yield return new OptionAimValue<bool>
            {
                Key = @"False",
                Description = @"Resist trip",
                Name = @"No",
                Value = false
            };
            yield break;
        }
        #endregion

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            if ((CounterAttemptChoice?.Selected as OptionAimValue<bool>)?.Value ?? false)
            {
                // counter-trip chosen
                if ((DropWeaponChoice?.Selected as OptionAimValue<bool>)?.Value ?? false)
                {
                    // find the first slot with this item
                    var _slot = _Target.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
                        .Where(_is => _is.SlottedItem == _OpponentWeapon).FirstOrDefault();
                    if (_slot != null)
                    {
                        // force a drop action
                        var _act = new CoreActivity(_Target, new DropHeldObject(_slot, string.Empty), null);
                        Process.ProcessManager.StartProcess(_act);
                    }

                    // exit early
                    return true;
                }

                // set-up trip checks
                AppendFollowing(new TripChecks(this, _Tripper, _Target));
            }
            return true;
        }
        #endregion
    }
}
