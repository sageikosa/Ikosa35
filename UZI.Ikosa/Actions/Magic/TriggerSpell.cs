using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Triggers a spell trigger spell [ActionBase]</summary>
    [Serializable]
    public class TriggerSpell : CastSpell
    {
        #region Construction
        /// <summary>Triggers a spell trigger spell [ActionBase]</summary>
        public TriggerSpell(CoreActor actor, SpellTrigger spellTrigger, ISpellMode mode, ActionTime actTime, string orderKey)
            : base(spellTrigger.ActorSpellSource(actor), mode, actTime, null, false, orderKey)
        {
            _Trigger = spellTrigger;
        }
        #endregion

        #region private data
        private SpellTrigger _Trigger;
        #endregion

        public SpellTrigger SpellTrigger => _Trigger;
        public override string Key => @"Item.TriggerSpell";
        public override string DisplayName(CoreActor actor)
            => $@"Trigger Spell: {SpellTrigger.SpellSource.DisplayName} ({SpellTrigger.CasterLevel})";

        public override string Description => SpellMode.Description;

        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
            => BaseCanPerformNow(budget);

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _info = base.GetActivityInfo(activity, observer);
            _info.Implement = GetInfoData.GetInfoFeedback(SpellTrigger.Anchor as ICoreObject, observer);
            return _info;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // NOTE: after spell trigger, some of the magic battery is consumed
            activity.EnqueueRegisterPreEmptively(Budget);

            // get sound
            var _critter = activity.Actor as Creature;
            if (_critter?.Languages.Any(_l => _l.CanProject) ?? false)
            {
                var _sound = new SpellCastSound(activity);
                if (_sound.IsSoundAudible(_critter.GetLocated()?.Locator))
                {
                    // sound!
                    _critter.AddAdjunct(GetActionSoundParticipant(activity, new SoundRef(_sound, 0, 120, _critter.GetSerialState())));
                    return new SpellTriggerStep(activity);
                }
                else
                {
                    // silence!
                    return activity.GetActivityResultNotifyStep(@"Words fail to come");
                }
            }
            return activity.GetActivityResultNotifyStep(@"Unable to speak");
        }
    }
}
