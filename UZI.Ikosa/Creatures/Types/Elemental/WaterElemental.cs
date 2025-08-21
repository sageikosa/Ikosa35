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
    public class WaterElemental : Species, IMonitorChange<int>
    {
        #region data
        protected Delta _NatArmor;
        protected Delta _Str;
        protected Delta _Dex;
        protected Delta _Con;
        #endregion

        #region size range and Slam damage
        protected readonly static Type[] _ClassSkills
           = new Type[] { typeof(SpotSkill), typeof(ListenSkill) };

        // make sure damage progression is setup
        private readonly static Dictionary<int, Roller> _SlamRollers;

        protected readonly static List<SizeRange> _SizeRanges;

        static WaterElemental()
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
            // base ability for water elementals
            var _set = new AbilitySet(10, 10, 10, 4, 11, 11);
            _Str = new Delta(4, typeof(Racial));
            _Dex = new Delta(0, typeof(Racial));
            _Con = new Delta(3, typeof(Racial));
            _set.Strength.Deltas.Add(_Str);
            _set.Dexterity.Deltas.Add(_Dex);
            _set.Constitution.Deltas.Add(_Con);
            return _set;
        }
        #endregion

        public override Species TemplateClone(Creature creature)
            => new WaterElemental();

        public override bool IsCharacterCapable => false;

        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            yield return new Aquan(this);
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
            var _class = new ElementalClass<WaterElemental>(_ClassSkills, 7, false, true, false, _SizeRanges);
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

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            yield return new LandMovement(20, Creature, this);
            yield return new SwimMovement(90, Creature, this, true, null); // no sink...
            yield break;
        }
        #endregion

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            foreach (var _trait in ElementalType.ElementalTraits(this))
            {
                yield return _trait;
            }

            // TODO: drench: snuff flames, dispel magic fire
            // TODO: vortex: shape-change; smaller creatures=save or damage; 
            //               smaller creatures=save or caught, damage each round; save each round to escape; 
            //               penalties and concentration checks; debris field
            // TODO: water mastery (+1 if both touching water, -4 if either touching ground)

            // slam
            yield return new ExtraordinaryTrait(this, @"Slam", @"Attack with slam", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, new Slam(@"1d8", Size.Medium, _SlamRollers, 20, 2, true,
                    Contracts.DamageType.Bludgeoning, false)));
            yield break;
        }

        #region protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        {
            switch (powerDie.Level)
            {
                case 1: return new PowerAttackFeat(powerDie, 1);
                case 3: return new CleaveFeat(powerDie, 3);
                case 6: return new GreatCleaveFeat(powerDie, 6);
                case 9: return new AlertnessFeat(powerDie, 9);
                case 12: return new ImprovedBullRushFeat(powerDie, 12);
                case 15: return new IronWillFeat(powerDie, 15);
                case 18: return new LightningReflexesFeat(powerDie, 18);
                case 21: return new ImprovedSunderFeat(powerDie, 21);
                case 24: return new ImprovedCriticalFeat<Slam>(powerDie, 24);
            }
            return null;
        }
        #endregion

        protected override Body GenerateBody()
        {
            _NatArmor = new Delta(6, typeof(Racial));
            var _body = new ElementalBody(ElementalMaterial.Static, Size.Small, 1);
            _body.NaturalArmor.Deltas.Add(_NatArmor);
            return _body;
        }

        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Shape", true, @"Vortical");
        }

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            var _reverse = _ClassSkills.Reverse().ToArray();
            var _pts = new int[] { 1, 1 };
            for (var _l = minLevel; _l <= maxLevel; _l++)
            {
                if ((_l % 2) == 1)
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
            var _deltas = (armor: 6, str: 4, dex: 0, con: 3);
            switch (args.NewValue)
            {
                case 1:
                case 2: break;
                case 3: _deltas = (7, 4, 0, 5); break;
                case 4:
                case 5: _deltas = (8, 6, 2, 7); break;
                case 6:
                case 7: _deltas = (8, 8, 2, 7); break;
                case 8:
                case 9:
                case 10:
                case 11: _deltas = (9, 10, 4, 9); break;
                case 12:
                case 13:
                case 14:
                case 15: _deltas = (9, 12, 6, 9); break;
                case 16:
                case 17:
                case 18:
                case 19:
                case 20: _deltas = (9, 14, 8, 11); break;
                case 21:
                case 22:
                case 23: _deltas = (9, 16, 10, 11); break;
                default: _deltas = (9, 18, 12, 11); break;
            }
            _NatArmor.Value = _deltas.armor;
            _Str.Value = _deltas.str;
            _Dex.Value = _deltas.dex;
            _Con.Value = _deltas.con;
        }

        #endregion
    }

    [Serializable]
    public class WaterElementalLarge : WaterElemental
    {
        #region public override Abilities.AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for air elementals
            var _set = new AbilitySet(10, 10, 10, 6, 11, 11);
            _Str = new Delta(4, typeof(Racial));
            _Dex = new Delta(0, typeof(Racial));
            _Con = new Delta(3, typeof(Racial));
            _set.Strength.Deltas.Add(_Str);
            _set.Dexterity.Deltas.Add(_Dex);
            _set.Constitution.Deltas.Add(_Con);
            return _set;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            var _class = new ElementalClass<WaterElementalLarge>(_ClassSkills, 15, false, true, false, _SizeRanges);
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
    public class WaterElementalHuge : WaterElemental
    {
        #region public override Abilities.AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for air elementals
            var _set = new AbilitySet(10, 10, 10, 6, 11, 11);
            _Str = new Delta(4, typeof(Racial));
            _Dex = new Delta(0, typeof(Racial));
            _Con = new Delta(3, typeof(Racial));
            _set.Strength.Deltas.Add(_Str);
            _set.Dexterity.Deltas.Add(_Dex);
            _set.Constitution.Deltas.Add(_Con);
            return _set;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            var _class = new ElementalClass<WaterElementalHuge>(_ClassSkills, 20, false, true, false, _SizeRanges);
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
            yield return new SwimMovement(120, Creature, this, true, null); // no sink...
            yield break;
        }
        #endregion
    }

    [Serializable]
    public class WaterElementalGreater : WaterElemental
    {
        #region public override Abilities.AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for air elementals
            var _set = new AbilitySet(10, 10, 10, 8, 11, 11);
            _Str = new Delta(4, typeof(Racial));
            _Dex = new Delta(0, typeof(Racial));
            _Con = new Delta(3, typeof(Racial));
            _set.Strength.Deltas.Add(_Str);
            _set.Dexterity.Deltas.Add(_Dex);
            _set.Constitution.Deltas.Add(_Con);
            return _set;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            var _class = new ElementalClass<WaterElementalGreater>(_ClassSkills, 23, false, true, false, _SizeRanges);
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
            yield return new SwimMovement(120, Creature, this, true, null); // no sink...
            yield break;
        }
        #endregion
    }

    [Serializable]
    public class WaterElementalElder : WaterElemental
    {
        #region public override Abilities.AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for air elementals
            var _set = new AbilitySet(10, 10, 10, 10, 11, 11);
            _Str = new Delta(4, typeof(Racial));
            _Dex = new Delta(0, typeof(Racial));
            _Con = new Delta(3, typeof(Racial));
            _set.Strength.Deltas.Add(_Str);
            _set.Dexterity.Deltas.Add(_Dex);
            _set.Constitution.Deltas.Add(_Con);
            return _set;
        }
        #endregion

        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            var _class = new ElementalClass<WaterElementalElder>(_ClassSkills, 48, false, true, false, _SizeRanges);
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
            yield return new SwimMovement(120, Creature, this, true, null); // no sink...
            yield break;
        }
        #endregion
    }
}
