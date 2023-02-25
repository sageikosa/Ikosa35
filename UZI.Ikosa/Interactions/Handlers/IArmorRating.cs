using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Interactions
{
    public interface IArmorRating : IAdjunctable
    {
        int ArmorRating { get; }

        /// <summary>Items and objects might be attended, in which case the attendee's conditions are relevant</summary>
        void AttendeeAdjustments(IAttackSource source, AttackData attack);
    }

    public static class IArmorRatingHelper
    {
        public static int GetArmorRating(this IArmorRating self, Sizer sizer)
        {
            var _attended = self.Adjuncts.OfType<Attended>().FirstOrDefault();
            if (_attended == null)
            {
                // not carried or worn
                return 3 + sizer.Size.SizeDelta.Value; /* -- 10(-5[DEX=0])(-2[Helpless]) */
            }
            else
            {
                // carried or worn (part of object load)
                return 10 + sizer.Size.SizeDelta.Value + _attended.Creature.MaxDexterityToARBonus.Value;
            }
        }

        public static void DoAttendeeAdjustments(this IArmorRating self, IAttackSource source, AttackData attack)
        {
            var _attended = self.Adjuncts.OfType<Attended>().FirstOrDefault();
            if (_attended != null)
            {
                // adjust attack for creature attending conditions
                var _interact = new Interaction(attack.Attacker, source, _attended.Creature, attack);
                (new ConditionAttackHandler()).HandleInteraction(_interact);
            }
        }
    }
}
