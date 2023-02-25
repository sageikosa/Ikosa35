using System;

namespace Uzi.Ikosa.Items.Materials
{
    #region Earth: HP/inch=1, Hardness=2
    [Serializable]
    public class EarthMaterial : Material
    {
        public readonly static EarthMaterial Static = new EarthMaterial();
        public override int Hardness { get { return 2; } }
        public override string Name { get { return @"Earth"; } }
        public override int StructurePerInch { get { return 1; } }
        public override string SoundQuality => @"muted";
    }
    #endregion

    #region Stone:  HP/inch=15, Hardness=8
    /// <summary>Stone (HP/inch=15, Hardness=8)</summary>
    [Serializable]
    public class StoneMaterial : EarthMaterial
    {
        public new readonly static StoneMaterial Static = new StoneMaterial();
        public override int Hardness { get { return 8; } }
        public override string Name { get { return @"Stone"; } }
        public override int StructurePerInch { get { return 15; } }
        public override string SoundQuality => @"stony";
    };
    #endregion
}
