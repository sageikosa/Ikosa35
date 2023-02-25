using System;
using System.Collections;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Creatures.Types
{
    /// <summary>
    /// Undead creature type
    /// </summary>
    [Serializable]
    public class UndeadType : CreatureType
    {
        public UndeadType()
        {
            _Aura = new UndeadAuraAdjunct(this);
        }

        public override string Name => @"Undead";
        public override bool IsLiving => false;

        private UndeadAuraAdjunct _Aura;

        protected override void OnBind()
        {
            base.OnBind();
            Creature.AddAdjunct(_Aura);
        }

        protected override void OnUnBind()
        {
            Creature.RemoveAdjunct(_Aura);
            base.OnUnBind();
        }

        /// <summary>Death, Mind-Affecting</summary>
        public static IEnumerable<TraitBase> UndeadPowerImmunities(Species source)
        {
            // Immune (power descriptors): death and mind-affecting
            var _descriptBlock = new PowerDescriptorsBlocker(source, @"Immune", typeof(Death), typeof(MindAffecting));
            yield return new ExtraordinaryTrait(source, @"Power Immunities", @"Death and mind-affecting powers have no effect",
                TraitCategory.Quality, new AdjunctTrait(source, _descriptBlock));
            yield break;
        }

        /// <summary>Poison, Sleep, Stunned, Fatigued, Exhausted, Paralyzed, Diseased, Negative Levels</summary>
        public static IEnumerable<TraitBase> UndeadEffectImmunities(Species source)
        {
            // Immune to poison, sleep, paralysis, stunning, disease, fatigue, exhaustion
            var _immune = new MultiAdjunctBlocker(source, @"Condition Immunities",
                typeof(Poisoned), typeof(SleepEffect), typeof(StunnedEffect), typeof(NegativeLevel),
                typeof(Fatigued), typeof(Exhausted), typeof(ParalyzedEffect), typeof(Diseased)
                );
            yield return new ExtraordinaryTrait(source, @"Undead Immunities",
                @"Poison, sleep, stun, paralysis, disease, fatigue, exhaustion", TraitCategory.Quality,
                new AdjunctTrait(source, _immune));
            yield break;
        }

        /// <summary>Cannot be healed, 100% resistance to critical hits, no non-lethal damage, destroyed at 0 health points</summary>
        public static IEnumerable<TraitBase> UndeadUnhealth(Species source)
        {
            // Cannot naturally heal, nor be healed
            yield return new ExtraordinaryTrait(source, @"Unhealable", @"Cannot be healed",
                TraitCategory.Quality, new InteractHandlerTrait(source, new CreatureNoRecoverHealthPointHandler()));

            // Immune to critical hits
            yield return new ExtraordinaryTrait(source, @"Immune critical", @"Critical ignore chance 100%",
                TraitCategory.Quality, new InteractHandlerTrait(source, new CriticalFilterHandler(100)));

            // Immune to nonlethal damage
            // Destroyed when reduced to 0 health points or less
            yield return new ExtraordinaryTrait(source, @"Immune to non-lethal", @"Non-lethal is ignored",
                TraitCategory.Quality, new InteractHandlerTrait(source, new CreatureNonLivingDamageHandler()));
            yield break;
        }
    }
}
