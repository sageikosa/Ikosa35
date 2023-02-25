using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public enum UpDownAdjustment : sbyte
    {
        /// <summary>-2</summary>
        StraightDown = -2,
        /// <summary>-1</summary>
        Downward = -1,
        /// <summary>0</summary>
        Level = 0,
        /// <summary>+1</summary>
        Upward = 1,
        /// <summary>+2</summary>
        StraightUp = 2
    }

    public static class UpDownAdjustmentStatic
    {
        public static bool IsVertical(this UpDownAdjustment self)
            => self == UpDownAdjustment.StraightDown || self == UpDownAdjustment.StraightUp;

        public static bool IsDiagonal(this UpDownAdjustment self)
            => ((int)self % 2) == 1;
    }
}
