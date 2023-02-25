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
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class VioletFungus : Species, IPoisonProvider
    {
        #region public override AbilitySet DefaultAbilities()
        public override AbilitySet DefaultAbilities()
        {
            // base ability for a medium fungus
            var _set = new AbilitySet(14, 8, 16, 1, 11, 9);
            _set.Intelligence.IsNonAbility = true;
            return _set;
        }
        #endregion

        public override Species TemplateClone(Creature creature)
            => new VioletFungus();

        /// <summary>Fungi are not characters</summary>
        public override bool IsCharacterCapable => false;

        /// <summary>Fungi do not get boosts</summary>
        public override bool SupportsAbilityBoosts => false;

        public override decimal FractionalPowerDie => 1m;
        public override decimal SmallestPowerDie => 1m;

        protected override CreatureType GenerateCreatureType() => new PlantType();

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
            => new PlantClass<VioletFungus>(new Type[] { }, 6, FractionalPowerDie, SmallestPowerDie, false);
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            yield return new LandMovement(10, Creature, this);
            yield break;
        }
        #endregion

        protected override int GenerateNaturalArmor() => 4;

        #region protected override IEnumerable<Advancement.PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<Advancement.PowerDieCalcMethod> GeneratePowerDice()
        {
            yield return Advancement.PowerDieCalcMethod.Average;
            yield return Advancement.PowerDieCalcMethod.Average;
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            yield return new Vision(true, this);
            yield return new Hearing(this);
            yield break;
        }
        #endregion

        #region protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
        }
        #endregion

        #region protected override IEnumerable<KeyValuePair<Type, Core.Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Core.Delta>> GenerateSkillDeltas()
        {
            yield break;
        }
        #endregion

        protected override Body GenerateBody()
        {
            var _body = new PlantBody(SoftPlantMaterial.Static, Size.Medium, 1)
            {
                BaseHeight = 4,
                BaseWidth = 3,
                BaseLength = 3,
                BaseWeight = 120
            };
            return _body;
        }

        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Tentacles", true, @"Tentacles");
            yield return new BodyFeature(this, @"Coloration", true, @"Purpleish");
            yield return new BodyFeature(this, @"Appearance", true, @"Bulbous cylinder");
            yield return new BodyFeature(this, @"Surface", false, @"Soft rubbery surface");
            yield break;
        }

        protected override IEnumerable<Adjuncts.TraitBase> GenerateTraits()
        {
            // TODO: fungus traits

            // Immune mind-affecting (descriptor) and death attacks (descriptor)
            var _descriptBlock = new PowerDescriptorsBlocker(this, @"Immune", typeof(MindAffecting));
            yield return new ExtraordinaryTrait(this, @"Mindless Immunities", @"Mind-affecting",
                 TraitCategory.Quality, new AdjunctTrait(this, _descriptBlock));

            // Immune to poison, sleep, paralysis, stunning, disease, fatigue, exhaustion
            var _immune = new MultiAdjunctBlocker(this, @"Condition Immunities",
                typeof(Poisoned), typeof(SleepEffect), typeof(StunnedEffect),
                typeof(ParalyzedEffect)
                // TODO: immune to polymorph
                );
            yield return new ExtraordinaryTrait(this, @"Plant Immunities",
                @"Poison, sleep, stun, paralysis, polymorph", TraitCategory.Quality,
                new AdjunctTrait(this, _immune));

            // Immune to critical hits
            yield return new ExtraordinaryTrait(this, @"Immune critical", @"Critical ignore chance 100%",
                TraitCategory.Quality, new InteractHandlerTrait(this, new CriticalFilterHandler(100)));

            // poisonous
            var _poisonTrait = new PoisonTrait(this, new Poisonous(this));
            yield return new ExtraordinaryTrait(this, @"Poison", @"Poisonous Tentacle", TraitCategory.CombatHelper,
                _poisonTrait);

            // tentacle attacks (*4)
            for (var _tx = 1; _tx <= 4; _tx++)
            {
                yield return new ExtraordinaryTrait(this, @"Tentacle Slot", @"Natural Weapon Slot", TraitCategory.CombatHelper,
                    new ItemSlotTrait(this, ItemSlot.Tentacle, _tx.ToString(), false, false));

                var _tentacle = new Tentacle(@"1d6", Size.Small, 20, 2, ItemSlot.Tentacle, _tx.ToString(), true, 
                    Contracts.DamageType.BludgeonAndSlash, true);
                var _wpnPoison = new WeaponSecondarySpecialAttackResult(_poisonTrait, false, true)
                {
                    PoweredUp = true
                };
                _tentacle.MainHead.AddAdjunct(_wpnPoison);
                yield return new ExtraordinaryTrait(this, @"Tentacle", @"Attack with tentacle", TraitCategory.CombatHelper,
                    new NaturalWeaponTrait(this, _tentacle));
            }

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield break;
        }

        #region public Poison GetPoison()
        public Poison GetPoison()
        {
            var _strDmg = new AbilityPoisonDamage(MnemonicCode.Str, new DieRoller(4));
            var _conDmg = new AbilityPoisonDamage(MnemonicCode.Con, new DieRoller(4));
            var _dmg = new PoisonMultiDamage(_strDmg, _conDmg);

            // build poison
            var _class = Creature.Classes.Get<PlantClass<VioletFungus>>();
            var _difficulty = _class != null
                ? Creature.GetIntrinsicPowerDifficulty(_class, MnemonicCode.Con, typeof(PoisonTrait))
                : new Deltable(10);
            return new Poison(@"Fungal Poison", _dmg, _dmg,
                Poison.ActivationMethod.Injury, Poison.MaterialForm.Liquid, _difficulty);
        }
        #endregion
    }
}
