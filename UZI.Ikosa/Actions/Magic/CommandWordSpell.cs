using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Magic;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Command word spell activation [ActionBase]</summary>
    [Serializable]
    public class CommandWordSpell : CastSpell
    {
        public CommandWordSpell(SpellCommandWord commandWord, ActionTime actTime, string orderKey)
            : base(commandWord.SpellSource, commandWord.SpellMode, actTime, null, false, orderKey)
        {
            _CommandWord = commandWord;
        }

        #region private data
        private SpellCommandWord _CommandWord;
        #endregion

        public SpellCommandWord SpellCommandWord => _CommandWord;

        public override string Key => @"Item.CommandWordActivate";
        public override string DisplayName(CoreActor actor)
            => $@"Command Activate Spell: {SpellCommandWord.SpellSource.DisplayName} ({SpellCommandWord.CasterLevel})";
        public override string Description => SpellMode.Description;

        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
            => BaseCanPerformNow(budget);

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _info = base.GetActivityInfo(activity, observer);
            _info.Implement = GetInfoData.GetInfoFeedback(SpellCommandWord.Anchor as ICoreObject, observer);
            return _info;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
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
                    return new SpellCommandWordStep(activity);
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
