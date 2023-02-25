using System;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public enum LightShadeLevel : sbyte
    {
        Skip = -128,
        MagicDark = -1,
        None = 0,
        ExtentBoost = 1,
        FarShadow = 2,
        FarBoost = 3,
        NearShadow = 4,
        NearBoost = 5,
        Bright = 6,
        VeryBright = 7
    }
}
