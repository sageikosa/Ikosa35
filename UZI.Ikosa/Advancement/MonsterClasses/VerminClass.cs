using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Advancement.MonsterClasses
{
    [Serializable]
    public class VerminClass<CritterType> : MonsterClass<CritterType>
        where CritterType : Species
    {
        #region Construction
        public VerminClass(Type[] skills, int maxLevel, 
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(8, maxLevel, optionalFraction, smallestFraction, flexibleAspect)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = true;
            _HasGoodReflex = false;
            _HasGoodWill = false;
        }

        public VerminClass(Type[] skills, int maxLevel, 
            List<SizeRange> sizeRanges, 
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(8, maxLevel, sizeRanges, optionalFraction, smallestFraction, flexibleAspect)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = true;
            _HasGoodReflex = false;
            _HasGoodWill = false;
        }

        public VerminClass(Type[] skills, int maxLevel, 
            bool goodFortitude, bool goodReflex, bool goodWill, 
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(8, maxLevel, optionalFraction, smallestFraction, flexibleAspect)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = true;
            _HasGoodReflex = false;
            _HasGoodWill = false;
        }

        public VerminClass(Type[] skills, int maxLevel, 
            bool goodFortitude, bool goodReflex, bool goodWill, 
            List<SizeRange> sizeRanges, 
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(8, maxLevel, sizeRanges, optionalFraction, smallestFraction, flexibleAspect)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = true;
            _HasGoodReflex = false;
            _HasGoodWill = false;
        }
        #endregion

        #region private data
        private Type[] _ClassSkills;
        private bool _HasGoodFortitude;
        private bool _HasGoodReflex;
        private bool _HasGoodWill;
        #endregion

        public override IEnumerable<Type> ClassSkills()
        {
            return _ClassSkills.Select(_cs => _cs);
        }

        public override string ClassName { get { return @"Vermin"; } }
        public override int SkillPointsPerLevel { get { return 2; } }
        public override double BABProgression { get { return 0.75d; } }

        public override bool HasGoodFortitude { get { return _HasGoodFortitude; } }
        public override bool HasGoodReflex { get { return _HasGoodReflex; } }
        public override bool HasGoodWill { get { return _HasGoodWill; } }

    }
}
