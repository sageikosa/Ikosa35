using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Advancement.MonsterClasses
{
    [Serializable]
    public class UndeadClass<CritterType> : MonsterClass<CritterType>
        where CritterType : Species
    {
        #region Construction
        public UndeadClass(Type[] skills, int maxLevel,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(12, maxLevel, optionalFraction, smallestFraction, flexibleAspect)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = false;
            _HasGoodReflex = false;
            _HasGoodWill = true;
        }

        public UndeadClass(Type[] skills, int maxLevel,
            List<SizeRange> sizeRanges,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(12, maxLevel, sizeRanges, optionalFraction, smallestFraction, flexibleAspect)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = false;
            _HasGoodReflex = false;
            _HasGoodWill = true;
        }

        public UndeadClass(Type[] skills, int maxLevel,
            bool goodFortitude, bool goodReflex, bool goodWill,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(12, maxLevel, optionalFraction, smallestFraction, flexibleAspect)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = goodFortitude;
            _HasGoodReflex = goodReflex;
            _HasGoodWill = goodWill;
        }

        public UndeadClass(Type[] skills, int maxLevel,
            bool goodFortitude, bool goodReflex, bool goodWill,
            List<SizeRange> sizeRanges,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(12, maxLevel, sizeRanges, optionalFraction, smallestFraction, flexibleAspect)
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

        public override string ClassName => @"Undead";
        public override int SkillPointsPerLevel => 4;
        public override double BABProgression => 0.5d;

        public override bool HasGoodFortitude => _HasGoodFortitude;
        public override bool HasGoodReflex => _HasGoodReflex;
        public override bool HasGoodWill => _HasGoodWill;
    }
}
