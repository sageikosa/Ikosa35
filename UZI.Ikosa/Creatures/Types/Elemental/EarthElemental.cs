using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Creatures.SubTypes;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class EarthElemental : Species, IMonitorChange<int>
    {
        #region data
        protected Delta _NatArmor;
        protected Delta _Str;
        protected Delta _Con;
        #endregion

        #region size range and Slam damage
        protected readonly static Type[] _ClassSkills
           = new Type[] { typeof(SpotSkill), typeof(ListenSkill) };

        // make sure damage progression is setup
        private readonly static Dictionary<int, Roller> _SlamRollers;

        protected readonly static List<SizeRange> _SizeRanges;

        static EarthElemental()
        {
            // non-standard slam progression
            _SlamRollers = WeaponDamageRollers.BuildRollerProgression(
                @"", @"", @"", @"1d6", @"1d8", @"2d8", @"2d10", @"", @"");

            // TODO: size boosts for abilities and armor (varies by elemental)
            _SizeRanges =
            [
                new CustomSizeRange(1, 3, Size.Small, 1, 1, 0, 0, 0, 0),
                new CustomSizeRange(4, 7, Size.Medium, 1, 1, 0, 0, 0, 0),
                new CustomSizeRange(8, 15, Size.Large, 2, 2, 0, 0, 0, 0),
                new CustomSizeRange(16, 48, Size.Huge, 3, 3, 0, 0, 0, 0)
            ];
        }
        #endregion

        #region public override Abilities.AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for earth elementals
            var _set = new AbilitySet(10, 8, 10, 4, 11, 11);
            _Str = new Delta(5, typeof(Racial));
            _Con = new Delta(1, typeof(Racial));
            _set.Strength.Deltas.Add(_Str);
            _set.Constitution.Deltas.Add(_Con);
            return _set;
        }
        #endregion

        public override Species TemplateClone(Creature creature)
            => new EarthElemental();

        public override bool IsCharacterCapable => false;

        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Terran(this);
            yield break;
        }

        protected override CreatureType GenerateCreatureType()
            => new ElementalType();

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield return new AirSubType(this);
            yield return new ExtraplanarSubType(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<SensoryBase> GenerateSenses()
        protected override IEnumerable<SensoryBase> GenerateSenses()
        {
            yield return new Darkvision(60, this);
            yield return new Vision(true, this);
            yield return new Hearing(this);
            yield break;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            var _class = new ElementalClass<EarthElemental>(_ClassSkills, 7, false, true, false, _SizeRanges);
            _class.AddChangeMonitor(this);
            return _class;
        }

        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
            => Enumerable.Range(1, 2).Select(_n => PowerDieCalcMethod.Average);

        public override bool SupportsAbilityBoosts => false;

        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
            => null;

        protected override int GenerateNaturalArmor()
            => 0;

        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            yield break;
        }

        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            yield return new LandMovement(20, Creature, this);
            // TODO: earth-glide: non-sinking swim through non-metals
            yield break;
        }

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            foreach (var _trait in ElementalType.ElementalTraits(this))
            {
                yield return _trait;
            }

            // TODO: earth mastery: +1 if both touch ground, -4 if opponent airborne or waterborne
            // TODO: push: bull rush without provoking attack, earth mastery applies

            // slam
            yield return new ExtraordinaryTrait(this, @"Slam", @"Attack with slam", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, new Slam(@"1d8", Size.Medium, _SlamRollers, 20, 2, true,
                    Contracts.DamageType.Bludgeoning, false)));
            yield break;
        }

        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        {
            switch (powerDie.Level)
            {
                case 1: return new PowerAttackFeat(powerDie, 1);
                case 3: return new CleaveFeat(powerDie, 3);
                case 6: return new GreatCleaveFeat(powerDie, 6);
                case 9: return new IronWillFeat(powerDie, 9);
                case 12: return new ImprovedBullRushFeat(powerDie, 12);
                case 15: return new AwesomeBlowFeat(powerDie, 15);
                case 18: return new AlertnessFeat(powerDie, 18);
                case 21: return new ImprovedSunderFeat(powerDie, 21);
                case 24: return new ImprovedCriticalFeat<Slam>(powerDie, 24);
            }
            return null;
        }

        protected override Body GenerateBody()
        {
            _NatArmor = new Delta(6, typeof(Racial));
            var _body = new ElementalBody(ElementalMaterial.Static, Size.Small, 1);
            _body.NaturalArmor.Deltas.Add(_NatArmor);
            return _body;
        }

        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Shape", true, @"Solidly Blocky");
        }

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            var _reverse = _ClassSkills.Reverse().ToArray();
            var _pts = new int[] { 1, 1 };
            for (var _l = minLevel; _l <= maxLevel; _l++)
            {
                if ((_l % 2) == 0)
                {
                    // listen then spot
                    Creature.AdvancementLog.DistributeSkillPoints(_l, _l, _reverse, _pts);
                }
                else
                {
                    // spot then listen
                    Creature.AdvancementLog.DistributeSkillPoints(_l, _l, _ClassSkills, _pts);
                }
            }
        }
        #endregion

        #region IMonitorChange<int> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<int> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<int> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<int> args)
        {
            var _deltas = (armor: 6, str: 5, con: 1);
            switch (args.NewValue)
            {
                case 1: break;
                case 2: _deltas = (7, 7, 3); break;
                case 3: _deltas = (8, 9, 5); break;
                case 4:
                case 5: _deltas = (9, 11, 7); break;
                case 6:
                case 7: _deltas = (9, 13, 7); break;
                case 8:
                case 9:
                case 10:
                case 11: _deltas = (10, 15, 9); break;
                case 12:
                case 13:
                case 14:
                case 15: _deltas = (10, 17, 9); break;
                case 16:
                case 17:
                case 18: _deltas = (11, 19, 11); break;
                case 19:
                case 20: _deltas = (12, 19, 11); break;
                case 21:
                case 22: _deltas = (13, 21, 11); break;
                case 23: _deltas = (14, 21, 11); break;
                default: _deltas = (15, 23, 11); break;
            }
            _NatArmor.Value = _deltas.armor;
            _Str.Value = _deltas.str;
            _Con.Value = _deltas.con;
        }

        #endregion
    }

    [Serializable]
    public class EarthElementalLarge : EarthElemental
    {
        #region public override Abilities.AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for earth elementals
            var _set = new AbilitySet(10, 8, 10, 6, 11, 11);
            _Str = new Delta(5, typeof(Racial));
            _Con = new Delta(1, typeof(Racial));
            _set.Strength.Deltas.Add(_Str);
            _set.Constitution.Deltas.Add(_Con);
            return _set;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            var _class = new ElementalClass<EarthElementalLarge>(_ClassSkills, 15, false, true, false, _SizeRanges);
            _class.AddChangeMonitor(this);
            return _class;
        }

        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
            => Enumerable.Range(1, 8).Select(_n => PowerDieCalcMethod.Average);

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            foreach (var _trait in base.GenerateTraits())
            {
                yield return _trait;
            }
            yield return DamageReductionTrait.GetDRAllTrait(this, 5);
            yield break;
        }
    }

    [Serializable]
    public class EarthElementalHuge : EarthElemental
    {
        #region public override Abilities.AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for air elementals
            var _set = new AbilitySet(10, 8, 10, 6, 11, 11);
            _Str = new Delta(5, typeof(Racial));
            _Con = new Delta(1, typeof(Racial));
            _set.Strength.Deltas.Add(_Str);
            _set.Constitution.Deltas.Add(_Con);
            return _set;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            var _class = new ElementalClass<EarthElementalHuge>(_ClassSkills, 20, false, true, false, _SizeRanges);
            _class.AddChangeMonitor(this);
            return _class;
        }

        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
            => Enumerable.Range(1, 16).Select(_n => PowerDieCalcMethod.Average);

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            foreach (var _trait in base.GenerateTraits())
            {
                yield return _trait;
            }
            yield return DamageReductionTrait.GetDRAllTrait(this, 5);
            yield break;
        }

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            yield return new LandMovement(30, Creature, this);
            // TODO: earth-glide: non-sinking swim through non-metals
            yield break;
        }
        #endregion
    }

    [Serializable]
    public class EarthElementalGreater : EarthElemental
    {
        #region public override Abilities.AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for air elementals
            var _set = new AbilitySet(10, 8, 10, 8, 11, 11);
            _Str = new Delta(5, typeof(Racial));
            _Con = new Delta(1, typeof(Racial));
            _set.Strength.Deltas.Add(_Str);
            _set.Constitution.Deltas.Add(_Con);
            return _set;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            var _class = new ElementalClass<EarthElementalGreater>(_ClassSkills, 23, false, true, false, _SizeRanges);
            _class.AddChangeMonitor(this);
            return _class;
        }

        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
            => Enumerable.Range(1, 21).Select(_n => PowerDieCalcMethod.Average);

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            foreach (var _trait in base.GenerateTraits())
            {
                yield return _trait;
            }
            yield return DamageReductionTrait.GetDRAllTrait(this, 10);
            yield break;
        }

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            yield return new LandMovement(30, Creature, this);
            // TODO: earth-glide: non-sinking swim through non-metals
            yield break;
        }
        #endregion
    }

    [Serializable]
    public class EarthElementalElder : EarthElemental
    {
        #region public override Abilities.AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for air elementals
            var _set = new AbilitySet(10, 8, 10, 10, 11, 11);
            _Str = new Delta(5, typeof(Racial));
            _Con = new Delta(1, typeof(Racial));
            _set.Strength.Deltas.Add(_Str);
            _set.Constitution.Deltas.Add(_Con);
            return _set;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            var _class = new ElementalClass<EarthElementalElder>(_ClassSkills, 48, false, true, false, _SizeRanges);
            _class.AddChangeMonitor(this);
            return _class;
        }

        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
            => Enumerable.Range(1, 24).Select(_n => PowerDieCalcMethod.Average);

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            foreach (var _trait in base.GenerateTraits())
            {
                yield return _trait;
            }
            yield return DamageReductionTrait.GetDRAllTrait(this, 10);
            yield break;
        }

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            yield return new LandMovement(30, Creature, this);
            // TODO: earth-glide: non-sinking swim through non-metals
            yield break;
        }
        #endregion
    }
}
