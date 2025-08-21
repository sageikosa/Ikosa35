using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Magic;
using Uzi.Visualize;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Elf : BaseHumanoidSpecies, IWeaponProficiency
    {
        public Elf()
            : base()
        {
        }

        public override Species TemplateClone(Creature creature)
            => new Elf();

        public override Type FavoredClass() => typeof(Wizard);
        public override string Name => @"Elf";
        public override bool IsCharacterCapable => true;

        #region public override IEnumerable<Language> CommonLanguages()
        public override IEnumerable<Language> CommonLanguages()
        {
            yield return new Languages.Draconic(this);
            yield return new Languages.Gnollish(this);
            yield return new Languages.Gnome(this);
            yield return new Languages.Goblin(this);
            yield return new Languages.Orcish(this);
            yield return new Languages.Sylvan(this);
        }
        #endregion

        #region IWeaponProficiency Members
        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => false;

        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            // longsword, rapier, longbow (including composite longbow), and shortbow (including composite shortbow)
            if (typeof(Longsword).IsAssignableFrom(type) || typeof(Rapier).IsAssignableFrom(type) ||
                typeof(Longbow).IsAssignableFrom(type) || typeof(ShortBow).IsAssignableFrom(type))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWithWeapon(weapon.GetType(), powerLevel);

        public string Description
            => @"Longsword, Rapier, Longbow, and Shortbow";
        #endregion

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield return new CreatureSpeciesSubType<Elf>(this, @"Elf");
            yield break;
        }
        #endregion

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
        {
            return new HumanoidBody(HideMaterial.Static, Size.Medium, 1, false)
            {
                BaseHeight = 5,
                BaseWidth = 2.5,
                BaseLength = 2.5,
                BaseWeight = 150
            };
        }
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            var _land = new LandMovement(30, Creature, this);
            yield return _land;
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield return new SwimMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            // +2 racial bonus on Listen, Search, and Spot checks. An elf who merely passes within 5 feet of a secret or concealed door is entitled to a Search check to notice it as if she were actively looking for it. 
            var _skills = new Delta(2, this, @"Elf Racial Trait");
            yield return new KeyValuePair<Type, Delta>(typeof(ListenSkill), _skills);
            yield return new KeyValuePair<Type, Delta>(typeof(SearchSkill), _skills);
            yield return new KeyValuePair<Type, Delta>(typeof(SpotSkill), _skills);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            // Automatic Languages: Common and Elven. Bonus Languages: Draconic, Gnoll, Gnome, Goblin, Orc, and Sylvan. 
            yield return new Common(this);
            yield return new Elven(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<SensoryBase> GenerateSenses()
        protected override IEnumerable<SensoryBase> GenerateSenses()
        {
            yield return new Vision(true, this);
            yield return new Senses.Hearing(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // +2 racial saving throw bonus against enchantment spells or effects
            yield return new ExtraordinaryTrait(this, @"Elven Save Bonuses", @"+2 saving throw bonus against enchantment spells or effects",
                TraitCategory.Quality, new QualifyDeltaTrait(this, new ElfSaves(), Creature.FortitudeSave, Creature.WillSave, Creature.ReflexSave));

            // sleep immunity
            var _sleepImmune = new SpellImmunityHandler(true, typeof(Sleep));
            yield return new ExtraordinaryTrait(this, @"Elven Immunity", @"Immune to Sleep", TraitCategory.Quality, new InteractHandlerTrait(this, _sleepImmune));

            // proficiency
            yield return new ExtraordinaryTrait(this, @"Elven Proficiencies", @"Proficient with longsword, rapier, longbow and shortbow",
                TraitCategory.Quality, new WrappedWeaponProficiencyTrait(this, this));

            // ability deltas
            yield return new ExtraordinaryTrait(this, @"Elf Dexterity", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Dexterity));
            yield return new ExtraordinaryTrait(this, @"Elf Constitution", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Constitution));

            // breathes air
            yield return BreathesAir.GetTrait(this);

            // auto-search
            yield return new ExtraordinaryTrait(this, @"Elf Search", @"Automatically search for secret doors",
                TraitCategory.Quality, new AdjunctTrait(this, new ElfSearch(this)));
            yield break;
        }
        #endregion
    }

    [Serializable]
    /// <summary>+2 against enchantments.</summary>
    public class ElfSaves : IQualifyDelta
    {
        /// <summary>+2 against enchantments.</summary>
        public ElfSaves()
        {
            _Terminator = new TerminateController(this);
            _Delta = new QualifyingDelta(2, typeof(Racial), @"Save versus Enchantment");
        }

        private IDelta _Delta;
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // TODO: conditional (interaction) modifiers (?!?)
            var _magic = qualify?.Source as MagicPowerActionSource;
            if (_magic?.MagicStyle is Enchantment)
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
    public class ElfSearch : Adjunct, IPathDependent, IMonitorChange<IGeometricRegion>
    {
        public ElfSearch(object source)
            : base(source)
        {
        }

        public override object Clone()
            => new ElfSearch(Source);

        public void PathChanged(Pathed source)
        {
            // see if we are still locatable
            if (source is Located)
            {
                Locator.FindFirstLocator(Anchor)?.AddChangeMonitor(this);
            }
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Locator.FindFirstLocator(Anchor)?.AddChangeMonitor(this);
        }

        // IMonitorChange<IGeometricRegion>
        public void PreTestChange(object sender, AbortableChangeEventArgs<IGeometricRegion> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        {
            // perform a search when region changes...
            if ((Anchor is Creature _critter)
                && (_critter.GetLocated()?.Locator is Locator _loc))
            {
                // cube for aiming cell extended to +1 in every direction
                var _rgn = _loc.GeometricRegion;
                var _cell = _critter.GetAimCell(_rgn);
                IEnumerable<ICellLocation> _outermostCells()
                {
                    yield return _cell.Add(2, 0, 0);
                    yield return _cell.Add(-2, 0, 0);
                    yield return _cell.Add(0, 2, 0);
                    yield return _cell.Add(0, -2, 0);
                    yield return _cell.Add(0, 0, 2);
                    yield return _cell.Add(0, 0, -2);
                }
                var _cells = new CellList(new Cubic(_cell, new GeometricSize(3, 3, 3))
                    .OffsetCubic(AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow).AllCellLocations()
                    .Union(_outermostCells()), 0, 0, 0);
                var _planar = _critter.Senses.PlanarPresence;

                // searchable openables in cellList
                foreach (var _searchable in from _tLoc in _loc.MapContext.LocatorsInRegion(_cells, _planar)
                                            from _open in _tLoc.AllConnectedOf<IOpenable>()
                                            where _open.HasActiveAdjunct<Searchable>()
                                            let _obj = _open as ICoreObject
                                            where _obj != null
                                            select _obj)
                {
                    // roll a d20
                    var _roll = new Deltable(DieRoller.RollDie(_critter.ID, 20, $@"{_critter.Name} Elf-Search", @"Search check"));

                    // add in search skill
                    _roll.Deltas.Add(new SoftQualifiedDelta(_critter.Skills.Skill<SearchSkill>()));

                    // perform search
                    _searchable.HandleInteraction(
                        new Interaction(_critter, _critter, _searchable, new SearchData(_critter, _roll, true)));
                }
            }
        }
    }
}
