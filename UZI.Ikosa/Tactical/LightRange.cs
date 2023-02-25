namespace Uzi.Ikosa.Tactical
{
    public enum LightRange : byte
    {
        OutOfRange,
        ExtentBoost,
        FarShadow,
        FarBoost,
        NearShadow,
        NearBoost,
        Bright,
        /// <summary>Dazzles certain creatures</summary>
        VeryBright,
        /// <summary>Damages certain creatures; dazzles others</summary>
        Solar
    }
}