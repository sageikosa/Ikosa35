using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Advancement.MonsterClasses;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public abstract class MonsterClass<CritterSpecies> : BaseMonsterClass
        where CritterSpecies : Species
    {
        // default empty size range (no size changes)
        private static Collection<SizeRange> _DefaultRanges = [];

        #region Construction
        protected MonsterClass(byte powerDie, int maxLevel, decimal optionalFraction, decimal smallestFraction,
            bool flexibleAspect)
            : this(powerDie, maxLevel, _DefaultRanges, optionalFraction, smallestFraction, flexibleAspect)
        {
        }

        protected MonsterClass(byte powerDie, int maxLevel, IEnumerable<SizeRange> sizeRanges,
            decimal optionalFraction, decimal smallestFraction, bool flexibleAspect)
            : base(powerDie, maxLevel, sizeRanges, optionalFraction, smallestFraction, flexibleAspect)
        {
        }
        #endregion

        /// <summary>Cannot remove if the creature is still of the species</summary>
        public override bool CanRemove()
        {
            return !(Creature.Species is CritterSpecies);
        }
    }
}
