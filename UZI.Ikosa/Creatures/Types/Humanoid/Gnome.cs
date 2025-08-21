using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Gnome : BaseHumanoidSpecies, IWeaponProficiencyTreatment
    {
        public Gnome()
            : base()
        {
        }

        public override Species TemplateClone(Creature creature)
            => new Gnome();

        protected override void OnConnectSpecies()
        {
            // weapon familiarity: gnome hooked hammer as martial
            Creature.Proficiencies.Add(this);

            // TODO: remaining stuff

            base.OnConnectSpecies();
        }

        public override IEnumerable<Language> CommonLanguages()
        {
            yield return new Languages.Draconic(this);
            yield return new Languages.Gnollish(this);
            yield return new Languages.Gnome(this);
            yield return new Languages.Goblin(this);
            yield return new Languages.Orcish(this);
            yield return new Languages.Sylvan(this);
            yield break;
        }

        #region IWeaponProficiency Members
        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => false;

        public bool IsProficientWithWeapon(Type type, int powerLevel)
            => type.Equals(typeof(GnomeHookedHammer))
            && Creature.Proficiencies.IsProficientWith(WeaponProficiencyType.Martial, powerLevel);

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWithWeapon(weapon.GetType(), powerLevel);

        public string Description
            => @"Treats Gnome Hooked-Hammer as Martial";
        #endregion

        /// <summary>+1 versus Kobolds and Goblinoids</summary>
        public class GnomeAtk : IQualifyDelta
        {
            /// <summary>+1 versus Kobolds and Goblinoids</summary>
            public GnomeAtk()
            {
                _Terminator = new TerminateController(this);
                _Delta = new QualifyingDelta(1, typeof(Racial), @"Attacking Goblin/Kobold");
            }

            private IDelta _Delta;
            public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            {
                // target must be ranged kobold or goblinoid
                if (qualify?.Target is Creature _critter)
                {
                    if (_critter.SubTypes
                        .Any(_st => _st is CreatureSpeciesSubType<Goblin> || _st is CreatureSpeciesSubType<Kobold>))
                    {
                        yield return _Delta;
                    }
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

        #region IWeaponProficiencyTreatment Members

        public WeaponProficiencyType WeaponTreatment(Type weaponType, int powerLevel)
            => weaponType.Equals(typeof(GnomeHookedHammer))
            ? WeaponProficiencyType.Martial
            : WeaponProficiencyHelper.StandardType(weaponType);

        public WeaponProficiencyType WeaponTreatment(IWeapon weapon, int powerLevel)
            => WeaponTreatment(weapon.GetType(), powerLevel);

        #endregion

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield return new CreatureSpeciesSubType<Gnome>(this, @"Gnome");
            yield break;
        }
        #endregion

        protected override Body GenerateBody()
            => new HumanoidBody(HideMaterial.Static, Size.Small, 1, false)
            {
                BaseHeight = 3.5,
                BaseWidth = 1.5,
                BaseLength = 1.5,
                BaseWeight = 50
            };

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
            var _skill = new Delta(2, this, @"Gnome Racial Bonus");
            yield return new KeyValuePair<Type, Delta>(typeof(Skills.ListenSkill), _skill);
            yield return new KeyValuePair<Type, Delta>(typeof(Skills.CraftAlchemy), _skill);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Languages.Common(this);
            yield return new Languages.Gnome(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<SensoryBase> GenerateSenses()
        protected override IEnumerable<SensoryBase> GenerateSenses()
        {
            yield return new Vision(true, this);
            yield return new Hearing(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            yield return new ExtraordinaryTrait(this, @"Martial Treatment", @"Treats gnome hooked hammer as martial",
                TraitCategory.Quality, new WeaponProficiencyTreatmentTrait(this, this));

            yield return new ExtraordinaryTrait(this, @"Gnome Attack", @"+1 versus Kobolds and Goblinoids",
                TraitCategory.Quality, new QualifyDeltaTrait(this, new GnomeAtk(),
                Creature.MeleeDeltable, Creature.RangedDeltable));

            yield return new ExtraordinaryTrait(this, @"Gnome Dodge", @"+4 dodge against giant types",
                TraitCategory.Quality, new QualifyDeltaTrait(this, new Dwarf.DwarfDodge(),
                Creature.NormalArmorRating, Creature.TouchArmorRating, Creature.IncorporealArmorRating));

            // ability deltas
            yield return new ExtraordinaryTrait(this, @"Gnome Constitution", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Constitution));
            yield return new ExtraordinaryTrait(this, @"Gnome Strength", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Strength));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            // TODO: other stuff
            yield break;
        }
        #endregion
    }
}
