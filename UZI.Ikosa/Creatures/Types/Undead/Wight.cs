using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Creatures.Templates;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Wight : Species, IReplaceCreature, IEnergyDrainProvider
    {
        #region ctor()
        public Wight()
        {
        }

        public Wight(Creature original)
        {
            _Original = original;
        }
        #endregion

        public override Species TemplateClone(Creature creature)
            => new Wight(Original.TemplateClone(Name));

        private Creature _Original;
        public Creature Original => _Original;

        public bool IsAcquired => true;

        #region protected override void OnConnectSpecies()
        protected override void OnConnectSpecies()
        {
            base.OnConnectSpecies();
            if (_Original != null)
            {
                UnslotAllItems(_Original);
            }
        }
        #endregion

        #region protected override void OnDisconnectSpecies()
        protected override void OnDisconnectSpecies()
        {
            if (_Original != null)
            {
                TransferItems(Creature, _Original);
            }
            base.OnDisconnectSpecies();
        }
        #endregion

        #region _ClassSkills
        // 20 + 5@2 (+ 10@4)
        private readonly static Type[] _ClassSkills =
            new Type[]
            {
                typeof(StealthSkill),
                typeof(ListenSkill),
                typeof(SilentStealthSkill),
                typeof(SpotSkill)
            };
        #endregion

        #region public override AbilitySet DefaultAbilities()
        // default ability set for making a ghoul
        public override AbilitySet DefaultAbilities()
        {
            var _set = new AbilitySet(12, 12, 10, 11, 13, 14);
            _set[MnemonicCode.Con].IsNonAbility = true;
            return _set;
        }
        #endregion

        public string TemplateName => Name;

        public override string Name => @"Wight";

        public override bool IsCharacterCapable
            => false;

        protected override CreatureType GenerateCreatureType()
            => new UndeadType();

        #region protected override BaseMonsterClass GenerateBaseMonsterClass()
        protected override BaseMonsterClass GenerateBaseMonsterClass()
        {
            // need to watch power die changes to alter traits
            var _class = new UndeadClass<Wight>(_ClassSkills, 8, 0m, 0m, false);
            return _class;
        }
        #endregion

        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
            => ((powerDieLevel % 4) == 0)
            ? MnemonicCode.Cha
            : null;

        protected override int GenerateNaturalArmor()
            => 4;

        #region protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
        {
            yield break;
        }
        #endregion

        #region protected override Body GenerateBody()
        protected override Body GenerateBody()
        {
            var _body = new HumanoidBody(HideMaterial.Static, Size.Medium, 1, false)
            {
                BaseHeight = 5,
                BaseWidth = 5,
                BaseLength = 5,
                BaseWeight = 200
            };

            return _body;
        }
        #endregion

        #region protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            yield return new BodyFeature(this, @"Skin", false, @"Pale Taught Skin");
            yield return new BodyFeature(this, @"Stature", true, @"Hunched over");
            yield break;
        }
        #endregion

        #region protected override IEnumerable<MovementBase> GenerateMovements()
        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            // A ghoul’s base land speed is 30 feet, unless based off a small creature
            var _land = (_Original == null)
                ? new LandMovement(30, Creature, this)
                : new LandMovement((Original.Sizer.NaturalSize.Order < Size.Medium.Order) ? 20 : 30, Creature, this);
            yield return _land;
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
        {
            if (_Original == null)
            {
                yield return PowerDieCalcMethod.Average;
                yield return PowerDieCalcMethod.Average;
                yield return PowerDieCalcMethod.Average;
                yield return PowerDieCalcMethod.Average;
            }
            else
            {
                // half power dice --> wight min 4, max 8
                var _last = Math.Max(Math.Min(_Original.AdvancementLog.NumberPowerDice / 2, 8), 4);
                for (var _px = 0; _px < _last; _px++)
                {
                    yield return PowerDieCalcMethod.Average;
                }
            }
            yield break;
        }
        #endregion

        #region protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
        {
            switch (powerDie.Level)
            {
                case 1:
                    return new AlertnessFeat(powerDie, 1);

                case 3:
                    return new BlindFight(powerDie, 3);

                case 6:
                    return new ToughnessFeat(powerDie, 6);
            }
            return null;
        }
        #endregion

        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _ClassSkills, new int[] { 1, 1, 1, 1 });
        }

        #region protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            var _skill = new Delta(8, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(SilentStealthSkill), _skill);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Language> GenerateAutomaticLanguages()
        protected override IEnumerable<Language> GenerateAutomaticLanguages()
        {
            if (_Original == null)
            {
                yield return new Common(this);
            }
            else
            {
                foreach (var _lang in GenerateLanguageCopies(_Original))
                {
                    yield return _lang;
                }
            }
            yield break;
        }
        #endregion

        #region protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        protected override IEnumerable<Senses.SensoryBase> GenerateSenses()
        {
            //Darkvision out to 60 feet. Vision and Hearing as well
            yield return new Senses.Vision(false, this);
            yield return new Senses.Darkvision(60, this);
            yield return new Senses.Hearing(this);
            yield break;
        }
        #endregion

        #region protected override IEnumerable<TraitBase> GenerateTraits()
        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            // alignment=chaotic evil
            var _aligned = new AlignedCreature(Alignment.LawfulEvil);
            yield return new ExtraordinaryTrait(this, @"Alignment", @"Always lawful evil",
                TraitCategory.Quality, new AdjunctTrait(this, _aligned));

            foreach (var _trait in UndeadType.UndeadPowerImmunities(this))
            {
                yield return _trait;
            }

            foreach (var _trait in UndeadType.UndeadEffectImmunities(this))
            {
                yield return _trait;
            }

            foreach (var _trait in UndeadType.UndeadUnhealth(this))
            {
                yield return _trait;
            }

            // energy drain...
            var _energyDrainTrait = new EnergyDrainTrait(this, this);
            yield return new ExtraordinaryTrait(this, @"Energy Drain", @"Bestow negative level on slam", TraitCategory.CombatHelper,
                _energyDrainTrait);

            // wight needs item slots for secondary natural attacks
            yield return new ExtraordinaryTrait(this, @"Slam Slot", @"Natural Weapon Slot", TraitCategory.CombatHelper,
                new ItemSlotTrait(this, ItemSlot.SlamSlot, string.Empty, false, false));

            // slam attack (with energy drain)
            var _slam = new Slam(@"1d4", Size.Medium, 20, 2, true, Contracts.DamageType.Bludgeoning, false);
            var _wpnEnergyDrain = new WeaponSecondarySpecialAttackResult(_energyDrainTrait, false, true);
            _slam.MainHead.AddAdjunct(_wpnEnergyDrain);
            yield return new ExtraordinaryTrait(this, @"Slam", @"Attack with slam", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _slam));
            yield break;
        }
        #endregion

        #region IEnergyDrainProvider Members
        public IVolatileValue Difficulty =>
            Creature.GetIntrinsicPowerDifficulty(Creature.Classes.Get<UndeadClass<Wight>>(), MnemonicCode.Cha, typeof(EnergyDrainTrait));

        public Roller Levels => new ConstantRoller(1);

        public Creature Drainer => Creature;
        #endregion

        #region IReplaceCreature Members

        public bool CanGenerate
        {
            get
            {
                return (Original.CreatureType is HumanoidType)
                    && _Original.CreatureType.IsLiving;
            }
        }

        #endregion
    }
}
