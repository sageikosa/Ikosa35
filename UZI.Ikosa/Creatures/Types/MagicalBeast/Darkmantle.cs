using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.MonsterClasses;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class Darkmantle : Species
    {
        private readonly static Type[] _Skills = new Type[]
        {
            typeof(StealthSkill),
            typeof(ListenSkill),
            typeof(SpotSkill)
        };

        public override Species TemplateClone(Creature creature)
            => new Darkmantle();

        public override bool IsCharacterCapable => false;

        protected override string GenerateAbilityBoostMnemonic(int powerDieLevel)
            => string.Empty;

        protected override FeatBase GenerateAdvancementFeat(PowerDie powerDie)
            => powerDie.Level switch
            {
                1 => new ImprovedInitiativeFeat(this, 1),
                3 => new ToughnessFeat(this, 3),
                _ => null
            };

        protected override IEnumerable<Language> GenerateAutomaticLanguages()
            => Enumerable.Empty<Language>();

        protected override BaseMonsterClass GenerateBaseMonsterClass()
            => new MagicalBeastClass<Darkmantle>(_Skills, 3, FractionalPowerDie, SmallestPowerDie, false);

        protected override CreatureType GenerateCreatureType()
            => new MagicalBeastType();

        protected override IEnumerable<CreatureSubType> GenerateSubTypes()
            => Enumerable.Empty<CreatureSubType>();

        protected override IEnumerable<MovementBase> GenerateMovements()
        {
            yield return new FlightExMovement(30, Creature, this, FlightManeuverability.Poor);

            var _land = new LandMovement(20, Creature, this);
            yield return _land;

            // needs this to hang from ceiling!
            yield return new ClimbMovement(0, Creature, this, false, _land);
            yield break;
        }

        protected override int GenerateNaturalArmor()
            => 6;

        protected override IEnumerable<PowerDieCalcMethod> GeneratePowerDice()
            => PowerDieCalcMethod.Average.ToEnumerable();

        protected override IEnumerable<SensoryBase> GenerateSenses()
        {
            yield return new Senses.BlindSight(90, true, this);
            yield return new Senses.Hearing(this);
            yield break;
        }

        protected override IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas()
        {
            var _skill = new Delta(4, typeof(Racial));
            yield return new KeyValuePair<Type, Delta>(typeof(StealthSkill), _skill);
            yield return new KeyValuePair<Type, Delta>(typeof(ListenSkill), _skill);
            yield return new KeyValuePair<Type, Delta>(typeof(SpotSkill), _skill);
            yield break;
        }

        protected override void GenerateSkillPoints(int minLevel, int maxLevel)
        {
            // stealth      10  =4Rc  +4Sz  +2SP
            // listen       5   =4Rc        +1SP
            // spot         5   =4Rc        +1SP
            Creature.AdvancementLog.DistributeSkillPoints(minLevel, maxLevel, _Skills, new int[] { 1, 1, 1 });
        }

        protected override Body GenerateBody()
        {
            // TODO: radial body
            throw new NotImplementedException();
        }

        protected override IEnumerable<BodyFeature> GenerateBodyFeatures()
        {
            // TODO: 
            throw new NotImplementedException();
        }

        protected override IEnumerable<TraitBase> GenerateTraits()
        {
            var _slam = new Slam(@"1d6", Size.Small, 20, 2, true, Contracts.DamageType.Bludgeoning, true);
            yield return new ExtraordinaryTrait(this, @"Slam", @"Attack with slam", TraitCategory.CombatHelper,
                new NaturalWeaponTrait(this, _slam));

            // TODO: improved grab
            // TODO: constrict
            // TODO: supernatural trait: darkness (1/day, CL 5th)
            // TODO: perhaps a qualified climb delta to hang from ceiling

            // breathes air
            yield return BreathesAir.GetTrait(this);
            yield return DailySleep.GetTrait(this);
            yield break;
        }
    }
}
