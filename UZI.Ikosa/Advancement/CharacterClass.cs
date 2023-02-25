using System;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public abstract class CharacterClass : AdvancementClass
    {
        #region construction
        public CharacterClass(byte powerDie, PowerDieCalcMethod initMethod)
            : base(powerDie)
        {
            _Method = initMethod;
        }

        public CharacterClass(byte powerDie, PowerDieCalcMethod initMethod, int maxLevel)
            : this(powerDie, initMethod)
        {
            _MaxLevel = maxLevel;
        }
        #endregion

        #region private data
        private PowerDieCalcMethod _Method;
        private int _MaxLevel = 20;
        #endregion

        /// <summary>
        /// Maximum level attainable for a single-class non-species PowerDie creature.  
        /// Note: may be less for multi-class characters
        /// </summary>
        public override int MaxLevel => _MaxLevel;

        /// <summary>Adds class to list.  Adds modifiers for BAB, Fort, Reflex and Will.  Increases Level to 1.</summary>
        protected override void OnAdd()
        {
            base.OnAdd();
            IncreaseLevel(_Method);
        }

    }
}