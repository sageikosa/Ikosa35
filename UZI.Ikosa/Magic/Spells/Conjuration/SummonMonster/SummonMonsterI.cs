using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class SummonMonsterI : SummonMonsterBase
    {
        public override IEnumerable<ISpellMode> SpellModes
            => new SummonMonsterMode(this, SummonSubMode.Regular).ToEnumerable();

        public override string DisplayName => @"Summon Monster I";
        public override string Description => @"Summon creature to fight for you";

        public override Creature GetCreature(CoreActor actor, OptionAimOption option, string prefix, int index)
        {
            // TODO: name generation based on index, and caster (and other tags from aiming) (and possibly other information)
            // TODO: devotions...

            switch (option.Key)
            {
                case @"1.COwl":
                    return GenerateCreature<Owl>(actor, @"Owl", prefix, index, @"Valor", LawChaosAxis.Lawful, GoodEvilAxis.Good, true, false);
                case @"1.CGiantFireBeetle":
                    return GenerateCreature<GiantFireBeetle>(actor, @"Giant Fire Beetle", prefix, index, @"Woodlands", LawChaosAxis.Neutral, GoodEvilAxis.Good, true, false);
                case @"1.CBadger":
                    // TODO:
                    return GenerateCreature<Monkey>(actor, @"Monkey", prefix, index, @"Heroism", LawChaosAxis.Chaotic, GoodEvilAxis.Good, true, false);
                case @"1.CMonkey":
                    return GenerateCreature<Monkey>(actor, @"Monkey", prefix, index, @"Heroism", LawChaosAxis.Chaotic, GoodEvilAxis.Good, true, false);
                case @"1.FDireRat":
                    return GenerateCreature<DireRat>(actor, @"Dire Rat", prefix, index, @"Tyranny", LawChaosAxis.Lawful, GoodEvilAxis.Evil, false, true);
                case @"1.FRaven":
                    // TODO:
                    return GenerateCreature<DireRat>(actor, @"Dire Rat", prefix, index, @"Tyranny", LawChaosAxis.Lawful, GoodEvilAxis.Evil, false, true);
                case @"1.FMonstrousCentipede":
                    return GenerateCreature<MonstrousCentipede>(actor, @"Monstrous Centipede", prefix, index, @"Dead", LawChaosAxis.Neutral, GoodEvilAxis.Evil, false, true);
                case @"1.FMonstrousScorpion":
                    // TODO: 
                    return GenerateCreature<MonstrousCentipede>(actor, @"Monstrous Centipede", prefix, index, @"Dead", LawChaosAxis.Neutral, GoodEvilAxis.Evil, false, true);
                case @"1.FHawk":
                    return GenerateCreature<Hawk>(actor, @"Hawk", prefix, index, @"Slaughter", LawChaosAxis.Chaotic, GoodEvilAxis.Evil, false, true);
                case @"1.FMonstrousSpider":
                    return GenerateCreature<MonstrousSpider>(actor, @"Monstrous Spider", prefix, index, @"Slaughter", LawChaosAxis.Chaotic, GoodEvilAxis.Evil, false, true);
                case @"1.FSnake":
                    return GenerateCreature<ViperSnake>(actor, @"Viper", prefix, index, @"Slaughter", LawChaosAxis.Chaotic, GoodEvilAxis.Evil, false, true);
                case @"1.CDog":
                default:
                    return GenerateCreature<Dog>(actor, @"Dog", prefix, index, @"Valor", LawChaosAxis.Lawful, GoodEvilAxis.Good, true, false);
            }
        }

        protected override IPowerDef NewForPowerSource()
            => new SummonMonsterI();

        public override IEnumerable<OptionAimOption> GetCreatures(CoreActor actor, SummonMonsterMode monsterMode)
            => GetMonsterList1(actor);

        protected IEnumerable<OptionAimOption> GetMonsterList1(CoreActor coreActor)
        {
            var _creature = coreActor as Creature;
            var _casterClasses = _creature.Classes.OfType<ICasterClass>().ToList();
            var _law = new Lawful();
            var _chaos = new Chaotic();
            var _good = new Good();
            var _evil = new Evil();

            if (CanSelect(_casterClasses, _law, _good))
            {
                yield return new OptionAimOption
                {
                    Key = @"1.CelestialDog",
                    Name = @"Celestial Dog",
                    Description = @"Lawful Good"
                };
                yield return new OptionAimOption
                {
                    Key = @"1.CelestialOwl",
                    Name = @"Celestial Owl",
                    Description = @"Lawful Good"
                };
            }
            if (CanSelect(_casterClasses, _good))
            {
                yield return new OptionAimOption
                {
                    Key = @"1.CelestialGiantFireBeetle",
                    Name = @"Celestial giant Fire Beetle",
                    Description = @"Good"
                };
                //yield return new OptionAimOption
                //{
                //    Key = @"1.CelestialPorpoise",
                //    Name = @"Celestial Porpoise",
                //    Description = @"Good"
                //};
            }
            if (CanSelect(_casterClasses, _chaos, _good))
            {
                yield return new OptionAimOption
                {
                    Key = @"1.CelestialBadger",
                    Name = @"Celestial Badger",
                    Description = @"Chaotic Good"
                };
                yield return new OptionAimOption
                {
                    Key = @"1.CelestialMonkey",
                    Name = @"Celestial Monkey",
                    Description = @"Chaotic Good"
                };
            }
            if (CanSelect(_casterClasses, _law, _evil))
            {
                yield return new OptionAimOption
                {
                    Key = @"1.FiendishDireRat",
                    Name = @"Fiendish Dire Rat",
                    Description = @"Lawful Evil"
                };
                yield return new OptionAimOption
                {
                    Key = @"1.FiendishRaven",
                    Name = @"Fiendish Raven",
                    Description = @"Lawful Evil"
                };
            }
            if (CanSelect(_casterClasses, _evil))
            {
                yield return new OptionAimOption
                {
                    Key = @"1.FiendishMonstrousCentipede",
                    Name = @"Fiendish Monstrous Centipede, Medium",
                    Description = @"Evil"
                };
                yield return new OptionAimOption
                {
                    Key = @"1.FiendishMonstrousScorpion",
                    Name = @"Fiendish Monstrous Scorpion, Small",
                    Description = @"Evil"
                };
            }
            if (CanSelect(_casterClasses, _chaos, _evil))
            {
                yield return new OptionAimOption
                {
                    Key = @"1.FiendishHawk",
                    Name = @"Fiendish Hawk",
                    Description = @"Chaotic Evil"
                };
                yield return new OptionAimOption
                {
                    Key = @"1.FiendishMonstrousSpider",
                    Name = @"Fiendish Monstrous Spider, Small",
                    Description = @"Chaotic Evil"
                };
                //yield return new OptionAimOption
                //{
                //    Key = @"1.FiendishOctopus",
                //    Name = @"Fiendish Octopus",
                //    Description = @"Chaotic Evil"
                //};
                yield return new OptionAimOption
                {
                    Key = @"1.FiendishSnake",
                    Name = @"Fiendish Snake, Small Viper",
                    Description = @"Chaotic Evil"
                };
            }

            yield break;
        }

        protected static bool CanSelect(List<ICasterClass> _casterClasses, params Descriptor[] descriptors)
        {
            // normally wouldn't block like this
            // but if one caster class is restricted from using alignment, then all should be
            if (_casterClasses.Any(_cc => descriptors.Any(_d => !_cc.CanUseDescriptor(_d))))
            {
                return false;
            }
            return true;
        }
    }
}
