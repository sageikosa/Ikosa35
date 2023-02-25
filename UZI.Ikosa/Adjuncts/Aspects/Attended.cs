using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Attended : Adjunct
    {
        // NOTE: currently being used when holding, slotted, or anchored (but not indirectly attached)
        public Attended(Creature critter)
            : base(critter)
        {
        }

        public Creature Creature => Source as Creature;
        public override bool IsProtected => true;
        public override object Clone() => new Attended(Creature);

        /// <summary>
        /// Tactical actions if actor is friendly and not totally concealed to creature, or if creature is helpless
        /// </summary>
        public bool AllowTacticalActions(Creature actor)
            => ((Creature?.IsFriendly(actor.ID) ?? true)
            && !(Creature?.Awarenesses.IsTotalConcealmentMiss(actor.ID) ?? false))
            || (Creature?.Conditions.Contains(Condition.Helpless) ?? true);
    }
}
