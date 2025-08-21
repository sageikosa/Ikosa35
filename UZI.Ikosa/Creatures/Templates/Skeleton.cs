using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.BodyType;

namespace Uzi.Ikosa.Creatures.Templates
{
    [Serializable]
    public class Skeleton : Species, IReplaceCreature
    {
        public Skeleton(Creature original)
            : base()
        {
            _Original = original;
        }

        public override Species TemplateClone(Creature creature)
            => new Skeleton(Original.TemplateClone(Name));

        public string TemplateName => Name;

        #region claw damage
        // make sure damage progression is setup
        private readonly static Dictionary<int, Roller> _ClawRollers;

        static Skeleton()
        {
            // non-standard claw progression
            _ClawRollers = WeaponDamageRollers.BuildRollerProgression(
                @"1", @"1", @"1d2", @"1d3", @"1d4", @"1d6", @"1d8", @"2d6", @"2d8");
        }
        #endregion

        private Creature _Original;
        public Creature Original => _Original;

        public bool IsAcquired => true;

        protected override void OnConnectSpecies()
        {
            base.OnConnectSpecies();
            TransferItems(_Original, Creature);
        }

        protected override void OnDisconnectSpecies()
        {
            TransferItems(Creature, _Original);
            base.OnDisconnectSpecies();
        }

        #region public override Abilities.AbilitySet DefaultAbilities()
        public override Abilities.AbilitySet DefaultAbilities()
        {
            // NOTE: strength will have no bonuses added
            var _strScore = _Original.Abilities.Strength.BaseValue;

            // NOTE: dexterity will have no bonuses added
            var _dexScore = _Original.Abilities.Dexterity.BaseValue;

            var _abilities = new Abilities.AbilitySet(_strScore, _dexScore, 10, 10, 10, 1);

            // NOTE: CON and INT are non abilities
            _abilities.Constitution.IsNonAbility = true;
            _abilities.Intelligence.IsNonAbility = true;
            return _abilities;
        }
        #endregion

        public override decimal FractionalPowerDie => _Original.Species.FractionalPowerDie;

        #region protected override Advancement.BaseMonsterClass GenerateBaseMonsterClass()
        protected override Advancement.BaseMonsterClass GenerateBaseMonsterClass()
        {
            // if no species classes, use these as defaults
            var _maxLevel = 1;
            var _fractional = 1.0m;

            // otherwise, get from species
            var _speciesClass = _Original.Classes.OfType<BaseMonsterClass>().FirstOrDefault();
            if (_speciesClass != null)
            {
                _maxLevel = _speciesClass.CurrentLevel;
                _fractional = _speciesClass.OptionalFraction;
            }

            // return
            return new UndeadClass<Skeleton>(Type.EmptyTypes, _maxLevel, _fractional, _fractional, false);
        }
        #endregion

        #region protected override BodyType.Body GenerateBody()
        protected override BodyType.Body GenerateBody()
        {
            // same body-type as base creature, except material is Bone
            var _body = _Original.Body.CloneBody(BoneMaterial.Static);

            // adjust
            _body.BaseWeight /= 8d;
            _body.Features.Clear();
            return _body;
        }
        #endregion

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            foreach (var _bodyFeature in _Original.Body.Features)
            {
                yield return new BodyFeature(this, _bodyFeature.Key, _bodyFeature.IsMajor, _bodyFeature.Description);
            }
            // TODO: any special skeletal features...
            // TODO: filter any features that shouldn't be shown...
            yield break;
        }
        #endregion

        protected override CreatureType GenerateCreatureType()
            => new UndeadType();

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            // same as base creature, except for alignment and "kind"
            foreach (var _sub in _Original.SubTypes)
            {
                if (!(_sub is BaseCreatureSpeciesSubType) && !(_sub is CreatureAlignmentSubType))
                {
                    yield return _sub.Clone(this);
                }
            }
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Movement.MovementBase> GenerateMovements()
        protected override IEnumerable<Movement.MovementBase> GenerateMovements()
        {
            // same as base creature: except flightEx, swim and climb
            foreach (var _move in _Original.Movements.AllMovements
                .Where(_m => (_m.Source is Species)
                && !(_m is SwimMovement) && !(_m is ClimbMovement) && !(_m is FlightExMovement)))
            {
                yield return _move.Clone(Creature, this);
            }

            // add climb and swim after all others (since they are based on land)
            foreach (var _move in _Original.Movements.AllMovements
                .Where(_m => (_m.Source is Species) && ((_m is SwimMovement) || (_m is ClimbMovement))))
            {
                yield return _move.Clone(Creature, this);
            }
            yield break;
        }
        #endregion

        #region protected override int GenerateNaturalArmor()
        protected override int GenerateNaturalArmor()
        {
            // skeleton natural armor "changes" based on size
            switch (_Original.Body.Sizer.NaturalSize.Order)
            {
                case -1: // Small
                    return 1;
                case 0:  // Medium
                case 1:  // Large
                    return 2;
                case 2:  // Huge
                    return 3;
                case 3:  // Gigantic
                    return 6;
                case 4:  // Colossal
                    return 10;
                default: // Tiny, Miniature, Fine
                    return 0;
            }
        }
        #endregion

        protected override bool GenerateSingleFractionalPowerDie()
            => _Original.AdvancementLog.Any()
            ? _Original.AdvancementLog[1].IsFractional
            : false;

        #region protected override IEnumerable<Advancement.PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<Advancement.PowerDieCalcMethod> GeneratePowerDice()
        {
            // species hit dice
            var _count = 0;
            foreach (var _pd in from _li in _Original.AdvancementLog
                                where _li.AdvancementClass is BaseMonsterClass
                                select _li.PowerDie)
            {
                yield return PowerDieCalcMethod.Average;
                _count++;
            }

            // at least one
            if (_count == 0)
            {
                yield return PowerDieCalcMethod.Average;
            }

            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            yield return new Darkvision(60, this);
            yield return new Vision(false, this);
            yield return new Hearing(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // alignment=neutral evil
            var _aligned = new AlignedCreature(Alignment.NeutralEvil);
            yield return new ExtraordinaryTrait(this, @"Alignment", @"Always neutral evil",
                TraitCategory.Quality, new AdjunctTrait(this, _aligned));

            // Immune (power descriptors): cold, death mind-affecting
            var _descriptBlock = new PowerDescriptorsBlocker(this, @"Immune", typeof(Cold), typeof(Death), typeof(MindAffecting));
            yield return new ExtraordinaryTrait(this, @"Power Immunities", @"Cold, death and mind-affecting powers have no effect",
                TraitCategory.Quality, new AdjunctTrait(this, _descriptBlock));

            // immune to cold damage
            yield return new ExtraordinaryTrait(this, @"Cold Immunity", @"No damage from cold",
                TraitCategory.Quality, new DeltaTrait(this, new Delta(Int16.MaxValue, typeof(EnergyResistance)),
                Creature.EnergyResistances[EnergyType.Cold]));

            // DR 5/bludgeoning
            yield return DamageReductionTrait.GetDRDamageTypeTrait(this, 5, Contracts.DamageType.Bludgeoning);

            foreach (var _trait in UndeadType.UndeadEffectImmunities(this))
            {
                yield return _trait;
            }

            foreach (var _trait in UndeadType.UndeadUnhealth(this))
            {
                yield return _trait;
            }

            // TODO: undead immunities

            // ability deltas
            yield return new ExtraordinaryTrait(this, @"Skeleton Dexterity", @"+2", TraitCategory.Quality,
                new DeltaTrait(this, new Delta(2, this, @"Racial Trait"), Creature.Abilities.Dexterity));

            // apply strength and dexterity deltas also
            foreach (var _boost in from _t in _Original.Traits
                                   where (_t.TraitCategory == TraitCategory.Quality) && (_t.Trait is DeltaTrait)
                                   let _d = _t.Trait as DeltaTrait
                                   where _d.Targets.Any(_trg => _trg is AbilityBase)
                                   select new { Delta = _d, Abilities = _d.Targets.OfType<AbilityBase>().ToList() })
            {
                if (_boost.Abilities.Any(_a => _a.Mnemonic == MnemonicCode.Str))
                {
                    yield return new ExtraordinaryTrait(this, @"Original Strength", _boost.Delta.Modifier.Value.ToString(),
                        TraitCategory.Quality, new DeltaTrait(this,
                            new Delta(_boost.Delta.Modifier.Value, _Original.Species, @"Original Trait"), Creature.Abilities.Strength));
                }
                else if (_boost.Abilities.Any(_a => _a.Mnemonic == MnemonicCode.Dex))
                {
                    yield return new ExtraordinaryTrait(this, @"Original Dexterity", _boost.Delta.Modifier.Value.ToString(),
                        TraitCategory.Quality, new DeltaTrait(this,
                            new Delta(_boost.Delta.Modifier.Value, _Original.Species, @"Original Trait"), Creature.Abilities.Dexterity));
                }
            }

            // will need claws
            var _clawTrack = new List<NaturalWeapon>();

            // combat helpers (includes natural weapons and item slots)
            foreach (var _trait in _Original.Traits.Where(_t => _t.TraitCategory == TraitCategory.CombatHelper))
            {
                var _clone = _trait.Clone(this) as TraitBase;
                if (_clone.Trait is NaturalWeaponTrait _natWpnTrait)
                {
                    if (_natWpnTrait.Weapon is Claw)
                    {
                        // found a claw!
                        var _clawMax = _natWpnTrait.Weapon.MainHead
                            .BaseDamageRollers(new Interaction(null, null, null, null), string.Empty, 0)
                            .FirstOrDefault(_r => _r.Source == _natWpnTrait.Weapon.MainHead)
                            .Roller.MaxRoll;
                        if (_clawMax > _ClawRollers[_Original.Sizer.NaturalSize.Order].MaxRoll)
                        {
                            // use it
                            _clawTrack.Add(_natWpnTrait.Weapon);
                            yield return _clone;
                        }
                    }
                    else
                    {
                        yield return _clone;
                    }
                }
                else
                {
                    yield return _clone;
                }
            }

            // hands without matching claws? create claws
            foreach (var _hand in _Original.Body.ItemSlots.AllSlots.OfType<HoldingSlot>())
            {
                if (!_clawTrack.Any(_c => (_c.SlotSubType == _hand.SubType)))
                {
                    yield return new ExtraordinaryTrait(this, @"Claw", @"Attack with claw", TraitCategory.CombatHelper,
                        new NaturalWeaponTrait(this, new Claw(@"1d4", Size.Miniature, _ClawRollers, 20, 2, _hand.SubType,
                        true, Contracts.DamageType.PierceAndSlash, false)));
                }
            }

            // proficiencies as per base creature
            yield return new ExtraordinaryTrait(this, @"Weapon Proficiencies", @"Same as original living creature", TraitCategory.CombatHelper,
                new WrappedWeaponProficiencyTrait(this, _Original.Proficiencies));
            yield return new ExtraordinaryTrait(this, @"Shield Proficiencies", @"Same as original living creature", TraitCategory.CombatHelper,
                new WrappedShieldProficiencyTrait(this, _Original.Proficiencies));
            yield return new ExtraordinaryTrait(this, @"Armor Proficiencies", @"Same as original living creature", TraitCategory.CombatHelper,
                new WrappedArmorProficiencyTrait(this, _Original.Proficiencies));

            // improved initiative
            yield return BonusFeatTrait.GetExtraordinaryBonusFeat(this, TraitCategory.Quality,
                new ImprovedInitiativeFeat(this, 1));
            yield break;
        }
        #endregion

        protected override Feats.FeatBase GenerateAdvancementFeat(Advancement.PowerDie powerDie) => null;
        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel) => null;
        protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages() { yield break; }
        protected override IEnumerable<KeyValuePair<Type, Core.Delta>> GenerateSkillDeltas() { yield break; }
        protected override void GenerateSkillPoints(int minLevel, int maxLevel) { }
        public override bool IsCharacterCapable => false;
        public override bool SupportsAbilityBoosts => false;

        #region IReplaceCreature Members

        public bool CanGenerate
        {
            // TODO: check to make sure species power dice do no exceed 20
            get { return _Original.CreatureType.IsLiving && _Original.Body.HasBones; }
        }

        #endregion
    }
}