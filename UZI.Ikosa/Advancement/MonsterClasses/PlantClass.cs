using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Advancement.MonsterClasses
{
    [Serializable]
    public class PlantClass<CritterType> : MonsterClass<CritterType>
        where CritterType : Species
    {
        #region Construction
        public PlantClass(Type[] skills, int maxLevel,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(8, maxLevel, optionalFraction, smallestFraction, flexibleAspect)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = true;
            _HasGoodReflex = false;
            _HasGoodWill = false;
        }

        public PlantClass(Type[] skills, int maxLevel,
            List<SizeRange> sizeRanges,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(8, maxLevel, sizeRanges, optionalFraction, smallestFraction, flexibleAspect)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = true;
            _HasGoodReflex = false;
            _HasGoodWill = false;
        }

        public PlantClass(Type[] skills, int maxLevel,
            bool goodFortitude, bool goodReflex, bool goodWill,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(8, maxLevel, optionalFraction, smallestFraction, flexibleAspect)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = goodFortitude;
            _HasGoodReflex = goodReflex;
            _HasGoodWill = goodWill;
        }

        public PlantClass(Type[] skills, int maxLevel,
            bool goodFortitude, bool goodReflex, bool goodWill,
            List<SizeRange> sizeRanges,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(8, maxLevel, sizeRanges, optionalFraction, smallestFraction, flexibleAspect)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = goodFortitude;
            _HasGoodReflex = goodReflex;
            _HasGoodWill = goodWill;
        }
        #endregion

        #region private data
        private Type[] _ClassSkills;
        private bool _HasGoodFortitude;
        private bool _HasGoodReflex;
        private bool _HasGoodWill;
        #endregion

        public override IEnumerable<Type> ClassSkills()
            => _ClassSkills.Select(_cs => _cs);

        public override string ClassName => @"Plant";
        public override int SkillPointsPerLevel => 2;
        public override double BABProgression => 0.75d;

        public override bool HasGoodFortitude => _HasGoodFortitude;
        public override bool HasGoodReflex => _HasGoodReflex;
        public override bool HasGoodWill => _HasGoodWill;

    }
}
