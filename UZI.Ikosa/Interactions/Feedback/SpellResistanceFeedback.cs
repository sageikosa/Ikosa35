using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Actions;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Indicates that the caster needs to make a caster level check to overcome spell resistance.</summary>
    public class SpellResistanceFeedback : PrerequisiteFeedback
    {
        /// <summary>Indicates that the caster needs to make a caster level check to overcome spell resistance.</summary>
        public SpellResistanceFeedback(Interaction delivery, Creature target, PowerAffectTracker tracker)
            : base(delivery.InteractData)
        {
            _Delivery = delivery;

            _CastPreReq = new CasterLevelPrerequisite(SpellTransit.PowerSource, _Delivery, _Delivery.Actor,
                @"CasterLevel.Check", @"Spell Resistance", target.ID, tracker, target.SpellResistance, true);
        }

        #region state
        private CasterLevelPrerequisite _CastPreReq;
        private Interaction _Delivery;
        #endregion

        public CasterLevelPrerequisite CasterLevelPrerequisite => _CastPreReq;
        public PowerActionTransit<SpellSource> SpellTransit => Source as PowerActionTransit<SpellSource>;

        public override IEnumerable<StepPrerequisite> Prerequisites
            => _CastPreReq.ToEnumerable();
    }
}