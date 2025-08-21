using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class VerbalComponent : SpellComponent, IProcessFeedback
    {
        public override string ComponentName => @"Verbal";

        public override bool CanStartActivity(Creature caster)
            => true;

        public override void StartUse(CoreActivity activity)
        {
            if (activity?.Actor is Creature _critter)
            {
                // monitor for silenced
                _critter.AddIInteractHandler(this);

                // deafened must check spell failure
                if (_critter.Conditions.Any(_c => _c.Name == Condition.Deafened))
                {
                    // play the percentages (20%)
                    if (DieRoller.RollDie(activity.Actor.ID, 100, @"Verbal", @"Deafened Failure Chance", activity.Actor.ID) <= 20)
                    {
                        HasFailed = true;
                    }
                }

                // sound generator finalized by action
                var _sound = GetSoundData(activity);
                _critter.AddAdjunct(new SoundParticipant(this, new SoundGroup(this, new SoundRef(_sound, 0, 120, _critter.GetSerialState()))));
            }
        }

        public override void StopUse(CoreActivity activity)
        {
            if (activity?.Actor is Creature _critter)
            {
                // monitor for silenced
                _critter.RemoveIInteractHandler(this);

                // sound itself
                _critter.Adjuncts.OfType<SoundParticipant>().FirstOrDefault(_sp => _sp.Source == this)?.Eject();
            }
        }

        public override bool WillUseSucceed(CoreActivity activity)
        {
            var _critter = activity.Actor as Creature;
            if ((_critter?.Languages.Any(_l => _l.CanProject) ?? false)
                 && !_critter.HasAdjunct<Silenced>())
            {
                // can speak and not silenced
                return true;
            }
            return false;
        }

        protected SpellCastSound GetSoundData(CoreActivity activity)
            => new SpellCastSound(activity);

        #region IHandleInteraction (AddAdjunctData)
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // last feedback processor
            if (typeof(AddAdjunctData).Equals(interactType))
            {
                return true;
            }

            return false;
        }

        public void ProcessFeedback(Interaction workSet)
        {
            if (workSet?.InteractData is AddAdjunctData _adder
                && _adder.Adjunct is Silenced)
            {
                // getting silenced with verbal components will fail the spell
                HasFailed = true;
            }
        }
        #endregion
    }
}
