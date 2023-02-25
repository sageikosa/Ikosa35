using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Time
{
    /// <summary>Fill up to a quarter of total spell capacity for pre-charged slots.</summary>
    [Serializable]
    public class PrepareMoreSpells : ActionBase
    {
        public PrepareMoreSpells(Creature critter, IPreparedCasterClass caster)
            : this(critter, caster, new ActionTime(Minute.UnitFactor * 15, Contracts.TimeType.TimelineScheduling))
        {
        }

        public PrepareMoreSpells(Creature critter, IPreparedCasterClass caster, ActionTime actiontime)
            : base(critter, actiontime, true, false, @"202")
        {
            // NOTE: 1 hour for all spells
            // no less than 15 minutes for smaller proportion
            // otherwise, half hour for 1/2 spell slots, 3/4 hour for 3/4 spell slots (etc)
            _Caster = caster;
        }


        #region data
        private IPreparedCasterClass _Caster;
        #endregion

        public override string Key => @"Timeline.PrepareMoreSpells";
        public override string DisplayName(CoreActor actor) => @"Prepare More Spells";
        public IPreparedCasterClass Caster => _Caster;

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new PrepareSpellSlotAim(@"PrepareSpells.Slots", @"Spell Slots to Prepare", Caster, false);
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return null;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            var _time = activity.Actor?.GetCurrentTime() ?? 0;
            var _prepTarget = activity.GetFirstTarget<PrepareSpellSlotsTarget>(@"PrepareSpells.Slots");
            if (_prepTarget != null)
            {
                var _fill = new FillQuarterSpellSlotsStep(activity, Caster);
                activity.EnqueueActivityResultOnStep(_fill, $@"Spells prepared.");
                return _fill;
            }
            return activity.GetActivityResultNotifyStep($@"No prepare spell slots target provided");
        }
    }
}
