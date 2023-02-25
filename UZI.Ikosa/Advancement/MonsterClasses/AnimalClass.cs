using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Advancement.MonsterClasses
{
    [Serializable]
    public class AnimalClass<CritterType> : MonsterClass<CritterType>
        where CritterType : Species
    {
        #region Construction
        public AnimalClass(Type[] skills, int maxLevel,
            decimal optionalFraction, bool flexibleAspect)
            : base(8, maxLevel, optionalFraction, optionalFraction, flexibleAspect)
        {
            _Skills = skills;
            _GoodFort = true;
            _GoodRflx = true;
            _GoodWill = false;
        }

        public AnimalClass(Type[] skills, int maxLevel,
            List<SizeRange> sizeRanges,
            decimal optionalFraction, bool flexibleAspect)
            : base(8, maxLevel, sizeRanges, optionalFraction, optionalFraction, flexibleAspect)
        {
            _Skills = skills;
            _GoodFort = true;
            _GoodRflx = true;
            _GoodWill = false;
        }

        public AnimalClass(Type[] skills, int maxLevel,
            bool goodFortitude, bool goodReflex, bool goodWill,
            decimal optionalFraction, bool flexibleAspect)
            : base(8, maxLevel, optionalFraction, optionalFraction, flexibleAspect)
        {
            _Skills = skills;
            _GoodFort = goodFortitude;
            _GoodRflx = goodReflex;
            _GoodWill = goodWill;
        }

        public AnimalClass(Type[] skills, int maxLevel,
            bool goodFortitude, bool goodReflex, bool goodWill,
            List<SizeRange> sizeRanges,
            decimal optionalFraction, bool flexibleAspect)
            : base(8, maxLevel, sizeRanges, optionalFraction, optionalFraction, flexibleAspect)
        {
            _Skills = skills;
            _GoodFort = goodFortitude;
            _GoodRflx = goodReflex;
            _GoodWill = goodWill;
        }
        #endregion

        #region private data
        private Type[] _Skills;
        private bool _GoodFort;
        private bool _GoodRflx;
        private bool _GoodWill;
        #endregion

        public override IEnumerable<Type> ClassSkills()
        {
            return _Skills.Select(_cs => _cs);
        }

        public override string ClassName { get { return @"Animal"; } }
        public override int SkillPointsPerLevel { get { return 2; } }
        public override double BABProgression { get { return 0.75d; } }

        public override bool HasGoodFortitude { get { return _GoodFort; } }
        public override bool HasGoodReflex { get { return _GoodRflx; } }
        public override bool HasGoodWill { get { return _GoodWill; } }
    }
}
