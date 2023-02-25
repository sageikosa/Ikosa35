using System;

namespace Uzi.Ikosa.Advancement.MonsterClasses
{
    [Serializable]
    public class CustomSizeRange : SizeRange
    {
        public CustomSizeRange(int lowPowerDie, int highPowerDie, Size creatureSize, int reach, int longReach,
            int dexterityUp, int strengthUp, int constitutionUp, int naturalArmorUp)
            : base(lowPowerDie, highPowerDie, creatureSize, reach)
        {
            _DexterityUp = dexterityUp;
            _StrengthUp = strengthUp;
            _ConstitutionUp = constitutionUp;
            _NaturalArmorUp = naturalArmorUp;
            _LongReach = longReach;
        }

        #region private data
        private int _DexterityUp;
        private int _StrengthUp;
        private int _ConstitutionUp;
        private int _NaturalArmorUp;
        private int _LongReach;
        #endregion

        public override int DexterityUp { get { return _DexterityUp; } }
        public override int StrengthUp { get { return _StrengthUp; } }
        public override int ConstitutionUp { get { return _ConstitutionUp; } }

        public override int NaturalArmorUp(int oldValue)
        {
            return _NaturalArmorUp;
        }

        public override int NaturalReach(bool isLong)
        {
            if (isLong)
                return _LongReach;
            else
                return base.NaturalReach(false);
        }
    }
}