using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Senses;
using Uzi.Core.Dice;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Creatures.Types
{
    /// <summary>
    /// This is just a test class, its namespace and hierarchy classification should be different (probably)
    /// </summary>
    [Serializable]
    public class Dwarf : BaseHumanoidSpecies, IWeaponProficiencyTreatment
    {
        public Dwarf()
            : base()
        {
        }

        public override Species TemplateClone(Creature creature)
            => new Dwarf();

        public override Type FavoredClass() => typeof(Fighter);
        public override string Name => @"Dwarf";

        #region public override IEnumerable<Language> CommonLanguages()
        public override IEnumerable<Language> CommonLanguages()
        {
            yield return new Giant(this);
            yield return new Languages.Gnome(this);
            yield return new Languages.Goblin(this);
            yield return new Orcish(this);
            yield return new Terran(this);
            yield return new Undercommon(this);
            yield break;
        }
        #endregion

        #region IWeaponProficiency Members
        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
        {
            // no racial proficiencies
            return false;
        }

        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            // racial re-map of dwarven waraxe and dwarven urgosh to martial weapons
            if (type.Equals(typeof(DwarvenWaraxe)) || type.Equals(typeof(DwarvenUrgosh)))
            {
                return Creature.Proficiencies.IsProficientWith(WeaponProficiencyType.Martial, powerLevel);
            }
            return false;
        }

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWithWeapon(weapon.GetType(), powerLevel);

        public string Description
            => @"Treats Dwarven Waraxe and Urgosh as martial";
        #endregion

        /// <summary>+1 versus Orcs and Goblinoids</summary>
        [Serializable]
        public class DwarfAtk : IQualifyDelta
        {
            /// <summary>+1 versus Orcs and Goblinoids</summary>
            public DwarfAtk()
            {
                _Terminator = new TerminateController(this);
                _Delta = new QualifyingDelta(1, typeof(Racial), @"Attacking Orc/Goblin");
            }

            private IDelta _Delta;
            public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            {
                // target must be orc or goblinoid
                if (qualify?.Target is Creature _critter)
                {
                    if (_critter.SubTypes
                        .Any(_st => _st is CreatureSpeciesSubType<Goblin> || _st is CreatureSpeciesSubType<Orc>))
                    {
                        yield return _Delta;
                    }
                }

                // TODO: item attended by orc of goblinoid
                yield break;
            }

            #region ITerminating Members
            /// <summary>
            /// Tells all modifiable values using this modifier to release it.  
            /// Note: this does not destroy the modifier and it can be re-used.
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

        /// <summary>+2 against poison, spells and spell-like effects.</summary>
        [Serializable]
        public class DwarfSaves : IQualifyDelta
        {
            /// <summary>+2 against poison, spells and spell-like effects.</summary>
            public DwarfSaves()
            {
                _Terminator = new TerminateController(this);
                _Delta = new QualifyingDelta(2, typeof(Racial), @"Save versus Poison/Spell");
            }

            private IDelta _Delta;
            public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            {
                // TODO: spell-like...
                if ((qualify?.Source is Poison) || (qualify?.Source is SpellSource))
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

        /// <summary>+4 dodge against giant types</summary>
        [Serializable]
        public class DwarfDodge : IQualifyDelta
        {
            /// <summary>+4 dodge against giant types</summary>
            public DwarfDodge()
            {
                _Terminator = new TerminateController(this);
                _Delta = new QualifyingDelta(4, typeof(DwarfDodge), @"Dwarven Dodge versus Giant");
            }

            private IDelta _Delta;
            public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            {
                // this should be the dwarf
                if (!(qualify is Interaction _iAct))
                {
                    yield break;
                }
                else
                {
                    var _critter = qualify?.Target as Creature;
                    if (_critter?.CanDodge(_iAct) ?? false)
                    {
                        // opponent is a giant
                        if ((qualify.Source as Creature)?.CreatureType is GiantType)
                        {
                            yield return _Delta;
                        }
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
        {
            // racial re-map of dwarven waraxe and dwarven urgosh to martial weapons
            if (weaponType.Equals(typeof(DwarvenWaraxe)) || weaponType.Equals(typeof(DwarvenUrgosh)))
            {
                return WeaponProficiencyType.Martial;
            }
            return WeaponProficiencyHelper.StandardType(weaponType);
        }

        public WeaponProficiencyType WeaponTreatment(IWeapon weapon, int powerLevel)
            => WeaponTreatment(weapon.GetType(), powerLevel);

        #endregion

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield return new CreatureSpeciesSubType<Dwarf>(this, @"Dwarf");
            yield break;
        }
        #endregion

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
        {
            var _body = new HumanoidBody(HideMaterial.Static, Size.Medium, 1, true)
            {
                BaseHeight = 3.75,
                BaseWidth = 2,
                BaseLength = 2,
                BaseWeight = 130
            };
            return _body;
        }
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            var _land = new LandMovement(20, Creature, this) { NoEncumberancePenalty = true };
            yield return _land;
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield return new SwimMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            yield break;
        }

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            //Automatic Languages: Common and Dwarven. Bonus Languages: Giant, Gnome, Goblin, Orc, Terran, and Undercommon. 
            yield return new Common(this);
            yield return new Dwarven(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            // Darkvision out to 60 feet. Vision and Hearing as well
            yield return new Senses.Vision(false, this);
            yield return new Senses.Darkvision(60, this);
            yield return new Senses.Hearing(this);
            yield break;
        }
        #endregion

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            //+2 racial bonus on saving throws against poison. 
            //+2 racial bonus on saving throws against spells and spell-like effects.
            yield return new ExtraordinaryTrait(this, @"Dwarven Saves", @"+2 against poison, spells and spell-like effects",
                TraitCategory.Quality, new QualifyDeltaTrait(this, new DwarfSaves(),
                Creature.FortitudeSave, Creature.ReflexSave, Creature.WillSave));

            yield return new ExtraordinaryTrait(this, @"Dwarven Appraise/Craft", @"+2 appraise/craft stone and metal",
                TraitCategory.Quality, new QualifyDeltaTrait(this, new DwarfAppraiseCraftBoost(),
                Creature.Skills.Skill<CraftSkill<CraftArmorSmithing>>(),
                Creature.Skills.Skill<CraftSkill<CraftWeaponSmithing>>(),
                Creature.Skills.Skill<CraftSkill<CraftTrapMaking>>(),
                Creature.Skills.Skill<AppraiseSkill>()));

            // weapon proficiencies (familiarity)
            yield return new ExtraordinaryTrait(this, @"Dwarven Weapons", @"Treats Dwarven Waraxe and Urgosh as Martial Weapons",
                TraitCategory.Quality, new WeaponProficiencyTreatmentTrait(this, this));

            // TODO: conditional (interaction) modifiers
            //Stability: +4 bonus on ability checks made to resist being bull rushed or tripped when standing on the ground (but not when climbing, flying, riding, or otherwise not standing firmly on the ground). 

            //+1 racial bonus on attack rolls against orcs and goblinoids. 
            yield return new ExtraordinaryTrait(this, @"Dwarven Attack", @"+1 attack against orcs and goblinoids",
                 TraitCategory.Quality, new QualifyDeltaTrait(this, new DwarfAtk(),
                Creature.RangedDeltable, Creature.MeleeDeltable, Creature.OpposedDeltable));

            // +4 dodge bonus to Armor Rating against monsters of the giant type. 
            yield return new ExtraordinaryTrait(this, @"Dwarven Dodge", @"+4 dodge bonus to armor rating against giant types",
                TraitCategory.Quality, new QualifyDeltaTrait(this, new DwarfDodge(),
                Creature.NormalArmorRating, Creature.TouchArmorRating, Creature.IncorporealArmorRating));

            // ability deltas
            yield return new ExtraordinaryTrait(this, @"Dwarf Constitution", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Constitution));
            yield return new ExtraordinaryTrait(this, @"Dwarf Charisma", @"-2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(-2, this, @"Racial Trait"), Creature.Abilities.Charisma));

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);

            // TODO: dwarf can search to find stonework traps like a rogue
            // TODO: dwarf knows depth underground

            // Stonecunning: dwarf within 10 feet of searchable stonework auto-searches
            yield return new ExtraordinaryTrait(this, @"Dwarf Stone Search", @"Automatically search for searchable stone objects, find stone-work traps",
                TraitCategory.Quality, new AdjunctTrait(this, new DwarfSearch(this)));

            // Stonecunning: +2 search racial bonus versus stonework
            yield return new ExtraordinaryTrait(this, @"Dwarf Stonecunning", @"+2 search versus stone", TraitCategory.Quality,
                new QualifyDeltaTrait(this, new DwarfSearchBoost(), Creature.Skills.Skill<SearchSkill>()));
            yield break;
        }
    }

    [Serializable]
    public class DwarfSearch : Adjunct, IPathDependent, IMonitorChange<IGeometricRegion>, ITrapFinding
    {
        public DwarfSearch(object source)
            : base(source)
        {
        }

        public override object Clone()
            => new DwarfSearch(Source);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Locator.FindFirstLocator(Anchor)?.AddChangeMonitor(this);
        }

        public void PathChanged(Pathed source)
        {
            // see if we are still locatable
            if (source is Located)
            {
                Locator.FindFirstLocator(Anchor)?.AddChangeMonitor(this);
            }
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
                    yield return _cell.Add(3, 0, 0);
                    yield return _cell.Add(-3, 0, 0);
                    yield return _cell.Add(0, 3, 0);
                    yield return _cell.Add(0, -3, 0);
                    yield return _cell.Add(0, 0, 3);
                    yield return _cell.Add(0, 0, -3);
                }
                var _cells = new CellList(new Cubic(_cell, new GeometricSize(5, 5, 5))
                    .OffsetCubic(AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow)
                    .OffsetCubic(AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow)
                    .AllCellLocations().Union(_outermostCells()), 0, 0, 0);

                var _planar = _critter.Senses.PlanarPresence;

                // searchable any IObjectBase made of earth material in cellList
                foreach (var _searchable in from _tLoc in _loc.MapContext.LocatorsInRegion(_cells, _planar)
                                            from _obj in _tLoc.AllConnectedOf<IObjectBase>()
                                            where _obj.HasActiveAdjunct<Searchable>()
                                            && (_obj.ObjectMaterial is EarthMaterial)
                                            select _obj)
                {
                    // roll a d20
                    var _roll = new Deltable(DieRoller.RollDie(_critter.ID, 20, $@"{_critter.Name} Dwarf-Search", @"Search check"));

                    // add in search skill
                    _roll.Deltas.Add(new SoftQualifiedDelta(_critter.Skills.Skill<SearchSkill>()));

                    // perform search
                    _searchable.HandleInteraction(
                        new Interaction(_critter, _critter, _searchable, new SearchData(_critter, _roll, true)));
                }
            }
        }

        // trapfinding
        /// <summary>Stone Search does not confer disable capability</summary>
        public bool CanDisableTrap(ICoreObject coreObj)
            => false;

        public bool CanFindTrap(ICoreObject coreObj)
            => (coreObj as IObjectBase)?.ObjectMaterial is EarthMaterial;
    }

    /// <summary>+2 search versus stone</summary>
    [Serializable]
    public class DwarfSearchBoost : IQualifyDelta
    {
        /// <summary>+4 dodge against giant types</summary>
        public DwarfSearchBoost()
        {
            _Terminator = new TerminateController(this);
            _Delta = new QualifyingDelta(2, typeof(DwarfSearchBoost), @"Dwarven Racial Trait");
        }

        private IDelta _Delta;
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (!(qualify is Interaction))
            {
                yield break;
            }
            else
            {
                var _obj = qualify?.Target as IObjectBase;
                if (_obj?.ObjectMaterial is EarthMaterial)
                {
                    // sought object is earth material (typically stone)
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

    /// <summary>+2 search versus stone</summary>
    [Serializable]
    public class DwarfAppraiseCraftBoost : IQualifyDelta
    {
        /// <summary>+4 dodge against giant types</summary>
        public DwarfAppraiseCraftBoost()
        {
            _Terminator = new TerminateController(this);
            _Delta = new QualifyingDelta(2, typeof(DwarfSearchBoost), @"Dwarven Racial Trait");
        }

        private IDelta _Delta;
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (!(qualify is Interaction))
            {
                yield break;
            }
            else
            {
                if ((qualify?.Target is ObjectBase _obj)
                    && ((_obj.ObjectMaterial is EarthMaterial) || (_obj.ObjectMaterial is MetalMaterial)))
                {
                    // object is earth material (typically stone)
                    yield return _Delta;
                }
                else if ((qualify?.Target is IItemBase _item)
                    && ((_item.ItemMaterial is EarthMaterial) || (_item.ItemMaterial is MetalMaterial)))
                {
                    // item is earth material (typically stone)
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

}
