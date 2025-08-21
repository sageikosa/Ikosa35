using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class LowerSpellResistanceStep : CoreStep
    {
        public LowerSpellResistanceStep(Interaction delivery, Creature target, PowerAffectTracker tracker)
            : base((CoreProcess)null)
        {
            _Delivery = delivery;
            _Target = target;
            _Tracker = tracker;
            _Dispensing = true;
        }

        #region state
        private Interaction _Delivery;
        private Creature _Target;
        private PowerAffectTracker _Tracker;
        private bool _Dispensing;
        #endregion

        public ChoicePrerequisite LowerResistanceChoice
            => AllPrerequisites<ChoicePrerequisite>(@"SpellResistance.Lower").FirstOrDefault();

        public CasterLevelPrerequisite CasterLevelCheck
            => AllPrerequisites<CasterLevelPrerequisite>(@"CasterLevel.Check").FirstOrDefault();

        #region private IEnumerable<OptionAimOption> DecideToLower()
        private IEnumerable<OptionAimOption> DecideToLower()
        {
            yield return new OptionAimValue<bool>
            {
                Key = @"False",
                Description = @"Use Spell Resistance",
                Name = @"Resist",
                Value = false
            };
            yield return new OptionAimValue<bool>
            {
                Key = @"True",
                Description = @"Lower Spell Resistance",
                Name = @"Allow",
                Value = true
            };
            yield break;
        }
        #endregion

        public override bool IsDispensingPrerequisites => _Dispensing;

        protected override StepPrerequisite OnNextPrerequisite()
        {
            if (!IsDispensingPrerequisites)
            {
                return null;
            }

            var _spellSource = (_Delivery.InteractData as PowerActionTransit<SpellSource>).PowerSource;

            var _lowerChoice = LowerResistanceChoice;
            if (_lowerChoice == null)
            {
                // must decide to lower before even "worrying" about caster level check
                return new ChoicePrerequisite(_spellSource, _Target, _Delivery, _Target,
                    @"SpellResistance.Lower", @"Lower Spell Resistance", DecideToLower(), true);
            }

            if (!((_lowerChoice.Selected as OptionAimValue<bool>)?.Value ?? false))
            {
                // did not decide to lower, so caster check now
                if (CasterLevelCheck == null)
                {
                    return new CasterLevelPrerequisite(_spellSource, _Delivery, _Delivery.Actor,
                        @"CasterLevel.Check", @"Spell Resistance", _Target.ID, _Tracker, _Target.SpellResistance, true);
                }
            }

            // done
            _Dispensing = false;
            return null;
        }

        protected override bool OnDoStep()
        {
            // lower resistance set to false?
            if (LowerResistanceChoice?.Selected is OptionAimValue<bool> _choice
                && !_choice.Value)
            {
                if (!CasterLevelCheck.Success)
                {
                    // didn't over come non-lowered spell-resistance
                    if (Process is CoreActivity _activity)
                    {
                        _activity.Terminate(@"Failed to overcome spell resistance");
                    }
                    else
                    {
                        Process.AppendPreEmption(
                            new TerminationStep(Process, new SysNotify(@"Failed to overcome spell resistance"))
                            {
                                InfoReceivers = _Delivery.Actor.ID.ToEnumerable().ToArray()
                            });
                    }
                }
            }

            // continue as normal
            return true;
        }

    }
}
