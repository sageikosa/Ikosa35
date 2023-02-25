using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class ElementalType : CreatureType
    {
        public override string Name => @"Elemental";

        /// <summary>Immune to Poison, Sleep, Stunned, Paralyzed.  Not subject to critical hits or flanking.</summary>
        public static IEnumerable<TraitBase> ElementalTraits(Species source)
        {
            // Immune to poison, sleep, paralysis, stunning, disease, fatigue, exhaustion
            var _immune = new MultiAdjunctBlocker(source, @"Condition Immunities",
                typeof(Poisoned), typeof(SleepEffect), typeof(StunnedEffect), typeof(ParalyzedEffect));
            yield return new ExtraordinaryTrait(source, @"Elemental Immunities",
                @"Poison, sleep, stun, paralysis", TraitCategory.Quality,
                new AdjunctTrait(source, _immune));

            // Immune to critical hits
            yield return new ExtraordinaryTrait(source, @"Immune critical", @"Critical ignore chance 100%",
                TraitCategory.Quality, new InteractHandlerTrait(source, new CriticalFilterHandler(100)));

            // TODO: immune to flanking

            yield break;
        }
    }
}

