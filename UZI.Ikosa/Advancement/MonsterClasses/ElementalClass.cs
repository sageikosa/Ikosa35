using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Advancement.MonsterClasses
{
    [Serializable]
    public class ElementalClass<CritterType> : MonsterClass<CritterType>
        where CritterType : Species
    {
        #region ctor()
        public ElementalClass(Type[] skills, int maxLevel,
            bool goodFortitude, bool goodReflex, bool goodWill,
            List<SizeRange> sizeRanges)
            : base(8, maxLevel, sizeRanges, 1m, 1m, false)
        {
            _ClassSkills = skills;
            _HasGoodFortitude = true;
            _HasGoodReflex = false;
            _HasGoodWill = false;
        }
        #endregion

        #region data
        private Type[] _ClassSkills;
        private bool _HasGoodFortitude;
        private bool _HasGoodReflex;
        private bool _HasGoodWill;
        #endregion

        public override string ClassName => @"Elemental";
        public override int SkillPointsPerLevel => 2;
        public override double BABProgression => 0.75d;

        public override bool HasGoodFortitude => _HasGoodFortitude;
        public override bool HasGoodReflex => _HasGoodReflex;
        public override bool HasGoodWill => _HasGoodWill;
    }
}
