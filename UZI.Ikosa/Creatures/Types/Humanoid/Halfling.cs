using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using System.Linq;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Halfling : BaseHumanoidSpecies
    {
        public Halfling()
            : base()
        {
        }

        public override Species TemplateClone(Creature creature)
            => new Halfling();

        public override Type FavoredClass() => typeof(Rogue);
        public override string Name => @"Halfling";
        public override bool IsCharacterCapable => true;

        #region public override IEnumerable<Language> CommonLanguages()
        public override IEnumerable<Language> CommonLanguages()
        {
            yield return new Languages.Dwarven(this);
            yield return new Languages.Elven(this);
            yield return new Languages.Gnome(this);
            yield return new Languages.Goblin(this);
            yield return new Languages.Orcish(this);
        }
        #endregion

        [Serializable]
        /// <summary>+2 against fear effects.</summary>
        public class HalflingFearSaves : IQualifyDelta
        {
            /// <summary>+2 against fear effects.</summary>
            public HalflingFearSaves()
            {
                _Terminator = new TerminateController(this);
                _Delta = new QualifyingDelta(2, typeof(Racial), @"Save versus Fear");
            }

            private IDelta _Delta;
            public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            {
                // NOTE: what besides SpellTransit can carry a fear effect?
                if (((qualify as Interaction)?.InteractData is PowerActionTransit<PowerActionSource> _transit)
                    && _transit.PowerSource.PowerDef.Descriptors.OfType<Fear>().Any())
                {
                    yield return _Delta;
                }
                yield break;
            }

            #region ITerminating Members
            /// <summary>
            /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
            /// </summary>
            public void DoTerminate()
            {
                _Terminator.DoTerminate();
            }

            #region IControlTerminate Members
            private readonly TerminateController _Terminator;
            public void AddTerminateDependent(IDependOnTerminate subscriber)
            {
                _Terminator.AddTerminateDependent(subscriber);
            }

            public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            {
                _Terminator.RemoveTerminateDependent(subscriber);
            }

            public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
            #endregion
            #endregion
        }

        [Serializable]
        /// <summary>+1 racial bonus on attack rolls with thrown weapons and slings</summary>
        public class HalflingRangedAtk : IQualifyDelta
        {
            /// <summary>+1 racial bonus on attack rolls with thrown weapons and slings</summary>
            public HalflingRangedAtk()
            {
                _Terminator = new TerminateController(this);
                _Delta = new QualifyingDelta(1, typeof(Racial), @"Attack with Throwable/Sling");
            }

            private IDelta _Delta;
            public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            {
                // make sure its a ranged attack
                if (((qualify as Interaction)?.InteractData is RangedAttackData _atk)
                    && ((_atk.RangedSource is IThrowableWeapon) || (_atk.RangedSource is Sling)))
                {
                    // throwable weapon or sling
                    yield return _Delta;
                }
                yield break;
            }

            #region ITerminating Members
            /// <summary>
            /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
            /// </summary>
            public void DoTerminate()
            {
                _Terminator.DoTerminate();
            }

            #region IControlTerminate Members
            private readonly TerminateController _Terminator;
            public void AddTerminateDependent(IDependOnTerminate subscriber)
            {
                _Terminator.AddTerminateDependent(subscriber);
            }

            public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            {
                _Terminator.RemoveTerminateDependent(subscriber);
            }

            public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
            #endregion
            #endregion
        }

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield return new CreatureSpeciesSubType<Halfling>(this, @"Halfling");
            yield break;
        }
        #endregion

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
        {
            return new HumanoidBody(HideMaterial.Static, Size.Small, 1, false)
            {
                BaseHeight = 3.25,
                BaseWidth = 1.5,
                BaseLength = 1.5,
                BaseWeight = 40
            };
        }
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            var _land = new LandMovement(20, Creature, this);
            yield return _land;
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield return new SwimMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            // +2 Climb, Jump, and Silent Stealth
            var _skills = new Delta(2, this, @"Halfling Racial Trait");
            yield return new KeyValuePair<Type, Delta>(typeof(ClimbSkill), _skills);
            yield return new KeyValuePair<Type, Delta>(typeof(JumpSkill), _skills);
            yield return new KeyValuePair<Type, Delta>(typeof(SilentStealthSkill), _skills);
            yield return new KeyValuePair<Type, Delta>(typeof(ListenSkill), _skills);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Common(this);
            yield return new Languages.Halfling(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<SensoryBase> GenerateSenses()
        protected override IEnumerable<SensoryBase> GenerateSenses()
        {
            yield return new Vision(false, this);
            yield return new Senses.Hearing(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // +1 all saves
            yield return new ExtraordinaryTrait(this, @"Halfing Saves", @"+1 all saves",
                TraitCategory.Quality, new DeltaTrait(this, new Delta(1, this, @"Halfling Racial Trait"),
                Creature.FortitudeSave, Creature.WillSave, Creature.ReflexSave));

            // +2 saves versus fear
            yield return new ExtraordinaryTrait(this, @"Halfing Saves", @"+2 against fear",
                TraitCategory.Quality, new QualifyDeltaTrait(this, new HalflingFearSaves(),
                Creature.FortitudeSave, Creature.ReflexSave, Creature.WillSave));

            // +1 attack with thrown weapons and sling
            yield return new ExtraordinaryTrait(this, @"Halfing Throw", @"+1 attack with thrown or sling",
                 TraitCategory.Quality, new QualifyDeltaTrait(this, new HalflingRangedAtk(), Creature.RangedDeltable));

            // ability deltas
            yield return new ExtraordinaryTrait(this, @"Halfing Dexterity", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Dexterity));
            yield return new ExtraordinaryTrait(this, @"Halfing Strength", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Strength));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            yield break;
        }
        #endregion
    }
}
