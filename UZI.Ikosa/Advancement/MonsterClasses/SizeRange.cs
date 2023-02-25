using System;

namespace Uzi.Ikosa.Advancement.MonsterClasses
{
    /// <summary>
    /// Uses standard progression for Dexterity, Strength, Constitution and Natural Armor
    /// </summary>
    [Serializable]
    public class SizeRange
    {
        /// <summary>
        /// Uses standard progression for Dexterity, Strength, Constitution and Natural Armor
        /// </summary>
        public SizeRange(int lowPowerDie, int highPowerDie, Size creatureSize, int reach)
        {
            LowPowerDie = lowPowerDie;
            HighPowerDie = highPowerDie;
            CreatureSize = creatureSize;
            _Reach = reach;
        }

        #region private data
        private int _Reach;
        #endregion

        public readonly int LowPowerDie;
        public readonly int HighPowerDie;
        public readonly Size CreatureSize;

        public virtual int NaturalReach(bool isLong) { return _Reach; } 

        #region public virtual int DexterityUp { get; }
        /// <summary>
        /// Value added to Base Dexterity (typically negative or zero!) when increasing to this size
        /// </summary>
        public virtual int DexterityUp
        {
            get
            {
                if (CreatureSize.Order < Size.Gigantic.Order)
                    return -2;
                return 0;
            }
        }
        #endregion

        #region public virtual int StrengthUp { get; }
        /// <summary>
        /// Value added to Base Strength when increasing to this size
        /// </summary>
        public virtual int StrengthUp
        {
            get
            {
                if (CreatureSize.Order == Size.Miniature.Order)
                    return 0;
                else if (CreatureSize.Order == Size.Tiny.Order)
                    return 2;
                else if (CreatureSize.Order == Size.Small.Order)
                    return 4;
                else if (CreatureSize.Order == Size.Medium.Order)
                    return 4;
                return 8;
            }
        }
        #endregion

        #region public virtual int ConstitutionUp { get; }
        /// <summary>
        /// Value added to Base Constitution when increasing to this size
        /// </summary>
        public virtual int ConstitutionUp
        {
            get
            {
                if (CreatureSize.Order == Size.Medium.Order)
                    return 2;
                else if (CreatureSize.Order == Size.Large.Order)
                    return 4;
                else if (CreatureSize.Order == Size.Huge.Order)
                    return 4;
                else if (CreatureSize.Order == Size.Gigantic.Order)
                    return 4;
                else if (CreatureSize.Order == Size.Colossal.Order)
                    return 5;
                return 0;
            }
        }
        #endregion

        #region public virtual int NaturalArmorUp(int oldValue)
        public virtual int NaturalArmorUp(int oldValue)
        {
            if (oldValue <= 0)
                return 0;

            if (CreatureSize.Order == Size.Large.Order)
            {
                return 2;
            }
            else if (CreatureSize.Order == Size.Huge.Order)
            {
                return 3;
            }
            else if (CreatureSize.Order == Size.Gigantic.Order)
            {
                return 4;
            }
            else if (CreatureSize.Order == Size.Colossal.Order)
            {
                return 5;
            }
            return 0;
        }
        #endregion
    }
}
