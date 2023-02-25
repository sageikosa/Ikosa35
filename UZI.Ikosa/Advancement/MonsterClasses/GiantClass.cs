using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Advancement.MonsterClasses
{
    [Serializable]
    public class GiantClass<CritterType> : MonsterClass<CritterType>
        where CritterType : Species
    {
        #region Construction
        public GiantClass(Type[] skills, int maxLevel, bool flexibleAspect)
            : this(skills, maxLevel, true, false, false, flexibleAspect)
        {
        }

        public GiantClass(Type[] skills, int maxLevel,
            bool goodFortitude, bool goodReflex, bool goodWill, bool flexibleAspect)
            : base(8, maxLevel, 1m, 1m, flexibleAspect)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = goodFortitude;
            _HasGoodReflex = goodReflex;
            _HasGoodWill = goodWill;
        }

        public GiantClass(Type[] skills, int maxLevel,
            List<SizeRange> sizeRanges,
            decimal optionalFraction, bool flexibleAspect)
            : this(skills, maxLevel, true, false, false, sizeRanges, flexibleAspect)
        {
        }

        public GiantClass(Type[] skills, int maxLevel,
            bool goodFortitude, bool goodReflex, bool goodWill,
            List<SizeRange> sizeRanges, bool flexibleAspect)
            : base(8, maxLevel, sizeRanges, 1m, 1m, flexibleAspect)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = goodFortitude;
            _HasGoodReflex = goodReflex;
            _HasGoodWill = goodWill;
        }
        #endregion

        #region data
        private Type[] _ClassSkills;
        private bool _HasGoodFortitude;
        private bool _HasGoodReflex;
        private bool _HasGoodWill;
        #endregion

        public override IEnumerable<Type> ClassSkills()
            => _ClassSkills.Select(_cs => _cs);

        public override string ClassName => @"Giant";
        public override int SkillPointsPerLevel => 2;
        public override double BABProgression => 0.75d;

        public override bool HasGoodFortitude => _HasGoodFortitude;
        public override bool HasGoodReflex => _HasGoodReflex;
        public override bool HasGoodWill => _HasGoodWill;
    }
}
