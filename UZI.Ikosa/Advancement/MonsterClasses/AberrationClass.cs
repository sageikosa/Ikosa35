using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Advancement.MonsterClasses
{
    [Serializable]
    public class AberrationClass<CritterType> : MonsterClass<CritterType>
        where CritterType : Species
    {
        #region ctor(...)
        public AberrationClass(Type[] skills, int maxLevel,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(8, maxLevel, optionalFraction, smallestFraction, flexibleAspect)
        {
            _Skills = skills;
            _GoodFort = false;
            _GoodRflx = false;
            _GoodWill = true;
        }

        public AberrationClass(Type[] skills, int maxLevel,
            List<SizeRange> sizeRanges,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(8, maxLevel, sizeRanges, optionalFraction, smallestFraction, flexibleAspect)
        {
            _Skills = skills;
            _GoodFort = false;
            _GoodRflx = false;
            _GoodWill = true;
        }

        public AberrationClass(Type[] skills, int maxLevel,
            bool goodFortitude, bool goodReflex, bool goodWill,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(8, maxLevel, optionalFraction, smallestFraction, flexibleAspect)
        {
            _Skills = skills;
            _GoodFort = goodFortitude;
            _GoodRflx = goodReflex;
            _GoodWill = goodWill;
        }

        public AberrationClass(Type[] skills, int maxLevel,
            bool goodFortitude, bool goodReflex, bool goodWill,
            List<SizeRange> sizeRanges,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(8, maxLevel, sizeRanges, optionalFraction, smallestFraction, flexibleAspect)
        {
            _Skills = skills;
            _GoodFort = goodFortitude;
            _GoodRflx = goodReflex;
            _GoodWill = goodWill;
        }
        #endregion

        #region state
        private Type[] _Skills;
        private bool _GoodFort;
        private bool _GoodRflx;
        private bool _GoodWill;
        #endregion

        public override IEnumerable<Type> ClassSkills()
            => _Skills.Select(_cs => _cs);

        public override string ClassName => @"Aberration";
        public override int SkillPointsPerLevel => 2;
        public override double BABProgression => 0.75d;

        public override bool HasGoodFortitude => _GoodFort;
        public override bool HasGoodReflex => _GoodRflx;
        public override bool HasGoodWill => _GoodWill;
    }
}
