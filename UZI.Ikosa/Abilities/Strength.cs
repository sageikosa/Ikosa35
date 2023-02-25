using Newtonsoft.Json;
using System;

namespace Uzi.Ikosa.Abilities
{
    [Serializable]
    public class Strength : AbilityBase
    {
        [NonSerialized, JsonIgnore]
        private static readonly double[] psMaxLoadBase = 
            { 100, 115, 130, 150, 175, 
              200, 230, 260, 300, 350 };

        #region Construction
        public Strength(int seedValue)
            : base(seedValue, MnemonicCode.Str)
        {
        }

        /// <summary>
        /// Non-Ability Constructor
        /// </summary>
        public Strength()
            : base(MnemonicCode.Str)
        {
        }
        #endregion

        public double BaseLightLoadMax { get { return BaseMaxLoad / 3d; } }
        public double BaseMediumLoadMax { get { return BaseMaxLoad * 2d / 3d; } }

        public double BaseMaxLoad
        {
            get
            {
                // TODO: qualified value...?!?
                int _val = this.EffectiveValue;
                if (_val < 10)
                {
                    return _val * 10d;
                }
                else //if (_val >= 10)
                {
                    int _ax = _val % 10;
                    double _fact = Math.Floor(Convert.ToDouble(_val) / 10d) - 1d;
                    return psMaxLoadBase[_ax] * Math.Pow(4d, Convert.ToDouble(_fact));
                }
            }
        }
    }
}
