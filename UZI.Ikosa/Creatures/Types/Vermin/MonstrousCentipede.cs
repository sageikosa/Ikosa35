using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class MonstrousCentipede : Species, IPoisonProvider, IMonitorChange<int>
    {
        public override Species TemplateClone(Creature creature)
            => new MonstrousCentipede();

        #region size range, bite damage, and ¿poison damage?
        // make sure damage progression is setup
        private readonly static Dictionary<int, Roller> _BiteRollers;

        private readonly static List<SizeRange> _SizeRanges;

        static MonstrousCentipede()
        {
            // non-standard size range adjustments for spiders
            _SizeRanges =
            [
                new CustomSizeRange(-1, -1, Size.Tiny, 0, 0, 0, 0, 0, 0),
                new CustomSizeRange(0, 0, Size.Small, 1, 1, 0, 4, 0, 1),
                new CustomSizeRange(1, 2, Size.Medium, 1, 1, 0, 4, 0, 1),
                new CustomSizeRange(3, 5, Size.Large, 1, 1, 0, 4, 0, 1),
                new CustomSizeRange(6, 11, Size.Huge, 2, 2, 0, 4, 2, 3),
                new CustomSizeRange(12, 23, Size.Gigantic, 3, 3, 0, 6, 0, 4),
                new CustomSizeRange(24, 48, Size.Colossal, 4, 4, -2, 4, 0, 6)
            ];

            // non-standard bite progression
            _BiteRollers = WeaponDamageRollers.BuildRollerProgression(
                @"1", @"1d2", @"1d3", @"1d4", @"1d6", @"1d8", @"2d6", @"2d8", @"4d6");
        }
        #endregion

        #region public override AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for a tiny centipede
            var _set = new AbilitySet(1, 15, 10, 1, 10, 2);
            _set.Intelligence.IsNonAbility = true;
            return _set;
        }
        #endregion

        /// <summary>Centipedes are not characters</summary>
        public override bool IsCharacterCapable => false;

        /// <summary>Centipedes do not get boosts</summary>
        public override bool SupportsAbilityBoosts => false;

        /// <summary>The smallest centipede gets 1/4 PowerDie</summary>
        public override decimal FractionalPowerDie => 0.5m;

        public override decimal SmallestPowerDie => 0.25m;

        #region protected override CreatureType GenerateCreatureType()
        protected override CreatureType GenerateCreatureType()
            => new VerminType();
        #endregion

        #region protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Languages.Language> GenerateAutomaticLanguages()
        {
            yield break;
        }
        #endregion

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield break;
        }
        #endregion

        #region protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
        {
            // no ability boosting
            return null;
        }
        #endregion

        #region protected override Feats.FeatBase GenerateAdvancementFeat(Advancement.PowerDie powerDie)
        protected override Feats.FeatBase GenerateAdvancementFeat(Advancement.PowerDie powerDie)
        {
            return null;
        }
        #endregion

        #region protected override Advancement.BaseMonsterClass GenerateBaseMonsterClass()
        protected override Advancement.BaseMonsterClass GenerateBaseMonsterClass()
        {
            // need to watch power die changes to selectively remove or re-add bonus feat
            var _class = new VerminClass<MonstrousCentipede>(new Type[] { }, 47, _SizeRanges, FractionalPowerDie, SmallestPowerDie, false);
            _class.AddChangeMonitor(this);
            return _class;
        }
        #endregion

        #region protected override BodyType.Body GenerateBody()
        protected override Body GenerateBody()
            => new CentipedeBody(Creature)
            {
                BaseHeight = 1.25,
                BaseWidth = 1.25,
                BaseLength = 1.25
            };
        #endregion

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Body", true, @"Body with multiple segments");
            yield return new BodyFeature(this, @"Mouth", false, @"Pincer-like Fangs");
            yield return new BodyFeature(this, @"Legs", true, @"Many Legs");
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Movement.MovementBase> GenerateMovements()
        protected override IEnumerable<Movement.MovementBase> GenerateMovements()
        {
            // NOTE: land-movement is sourced by the body, as speed varies with size and type
            // NOTE: climb-movement varies by size
            yield break;
        }
        #endregion

        #region protected override int GenerateNaturalArmor()
        protected override int GenerateNaturalArmor()
        {
            // varies non-uniformly by size
            return 0;
        }
        #endregion

        #region protected override IEnumerable<Advancement.PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<Advancement.PowerDieCalcMethod> GeneratePowerDice()
        {
            yield return Advancement.PowerDieCalcMethod.Average;
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            yield return new Darkvision(60, this);
            yield return new Vision(true, this);
            yield return new Hearing(this);
            yield break;
        }
        #endregion

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            // NONE: centipedes have bonuses and ability modifiers to specific things
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Core.Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Core.Delta>> GenerateSkillDeltas()
        {
            // +4 Hide/Spot
            var _skill4 = new Delta(4, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(SpotSkill), _skill4);

            // +8 Climb
            var _skill8 = new Delta(8, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(ClimbSkill), _skill8);
            yield return new KeyValuePair<Type, Delta>(typeof(StealthSkill), _skill8);
            yield break;
        }
        #endregion

        protected override IEnumerable<Adjuncts.TraitBase> GenerateTraits()
        {
            // TODO: centipede traits

            // better of STR/DEX for Climb
            var _better = new BetterDelta(Creature.Abilities.Ability<Dexterity>(), Creature.Abilities.Ability<Strength>());
            yield return new ExtraordinaryTrait(this, @"Nimble Climber", @"Uses Dexterity for Climb if better then Strength",
                TraitCategory.Quality, new DeltaTrait(this, _better, Creature.Skills.Skill<ClimbSkill>()));

            // Immune mind-affecting (descriptor) and death attacks (descriptor)
            var _descriptBlock = new PowerDescriptorsBlocker(this, @"Immune", typeof(MindAffecting));
            yield return new ExtraordinaryTrait(this, @"Mindless Immunities", @"Mind-affecting",
                 TraitCategory.Quality, new AdjunctTrait(this, _descriptBlock));

            // poisonous
            var _poisonTrait = new PoisonTrait(this, new Poisonous(this));
            yield return new ExtraordinaryTrait(this, @"Poison", @"Poisonous Bite", TraitCategory.CombatHelper,
                _poisonTrait);

            // bite attack
            var _bite = new Bite(@"1d3", Size.Tiny, _BiteRollers, 20, 2, true, Contracts.DamageType.All, true);
            var _wpnPoison = new WeaponSecondarySpecialAttackResult(_poisonTrait, false, true)
            {
                PoweredUp = true
            };
            _bite.MainHead.AddAdjunct(_wpnPoison);
            yield return new ExtraordinaryTrait(this, @"Bite", @"Attack with bite", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _bite));

            // weapon finesse (bonus feat)
            yield return BonusFeatTrait.GetExtraordinaryBonusFeat(this, TraitCategory.Quality,
                new WeaponFinesse(this, 1) { IgnorePreRequisite = true });

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);
            yield break;
        }

        #region public Poison GetPoison()
        public Poison GetPoison()
        {
            // poison damage by size
            Roller _roller()
            {
                switch (Creature.Body.Sizer.Size.Order)
                {
                    case 4:  // colossal
                        return new DiceRoller(2, 6);

                    case 3:  // gigantic
                        return new DieRoller(8);

                    case 2:  // huge
                        return new DieRoller(6);

                    case 1:  // large
                        return new DieRoller(4);

                    case 0:  // medium
                        return new DieRoller(3);

                    case -1: // small
                        return new DieRoller(2);

                    case -2: // Tiny
                    default:
                        return new ConstantRoller(1);
                };
            }
            var _dmg = new AbilityPoisonDamage(MnemonicCode.Dex, _roller());

            // build poison
            var _class = Creature.Classes.Get<VerminClass<MonstrousCentipede>>();
            var _difficulty = _class != null
                ? Creature.GetIntrinsicPowerDifficulty(_class, MnemonicCode.Con, typeof(PoisonTrait))
                : new Deltable(10);
            return new Poison(@"Centipede Poison", _dmg, _dmg,
                Poison.ActivationMethod.Injury,
                Poison.MaterialForm.Liquid,
                _difficulty);
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
            if ((args.OldValue >= 6) && (args.NewValue < 6))
            {
                // going back to large...re-add weapon finesse
                Creature.AddAdjunct(BonusFeatTrait.GetExtraordinaryBonusFeat(this, TraitCategory.Quality,
                    new WeaponFinesse(this, 1) { IgnorePreRequisite = true }));
            }
            else if ((args.OldValue < 6) && (args.NewValue >= 6))
            {
                // going to huge...remove weapon finesse
                (from _ex in Creature.Adjuncts.OfType<ExtraordinaryTrait>()
                 let _feat = _ex.Trait as BonusFeatTrait
                 where _feat != null && _feat.Feat is WeaponFinesse
                 select _ex).FirstOrDefault()?.Eject();
            }
        }

        #endregion
    }
}
