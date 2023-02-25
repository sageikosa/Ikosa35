using System;

namespace Uzi.Ikosa.Items.Materials
{
    [Serializable]
    public class MetalMaterial : Material
    {
        public static MetalMaterial CommonStatic = new MetalMaterial();
        public override int Hardness => 10;
        public override string Name => @"Metal";
        public override int StructurePerInch => 30;
        public override string SoundQuality => @"metallic";
    }

    #region Iron:  HP/inch=30, Hardness=10
    /// <summary>Iron (HP/inch=30, Hardness=10)</summary>
    [Serializable]
    public class IronMaterial : MetalMaterial
    {
        public static IronMaterial Static = new IronMaterial();
        public override int Hardness => 10;
        public override string Name => @"Iron";
        public override int StructurePerInch => 30;
    };
    #endregion

    #region Steel:  HP/inch=30, Hardness=10
    /// <summary>Steel (HP/inch=30, Hardness=10)</summary>
    [Serializable]
    public class SteelMaterial : MetalMaterial
    {
        public static SteelMaterial Static = new SteelMaterial();
        public override int Hardness => 10;
        public override string Name => @"Steel";
        public override int StructurePerInch => 30;
    };
    #endregion

    #region Mithral:  HP/inch=30, Hardness=15
    /// <summary>Mithral (HP/inch=30, Hardness=15)</summary>
    [Serializable]
    public class MithralMaterial : MetalMaterial
    {
        public static MithralMaterial Static = new MithralMaterial();
        public override int Hardness => 15;
        public override string Name => @"Mithral";
        public override int StructurePerInch => 30;
    };
    #endregion

    #region Adamantine:  HP/inch=40, Hardness=20
    /// <summary>Adamantine (HP/inch=40, Hardness=20)</summary>
    [Serializable]
    public class AdamantineMaterial : MetalMaterial
    {
        public static AdamantineMaterial Static = new AdamantineMaterial();
        public override int Hardness => 20;
        public override string Name => @"Adamantine";
        public override int StructurePerInch => 40;
    };
    #endregion

    #region Alchemal Silver: HP/inch=30, Hardness=10
    /// <summary>Alchemal Silver (HP/inch=30, Hardness=10)</summary>
    [Serializable]
    public class SilverPlatingMaterial : MetalMaterial
    {
        public static SilverPlatingMaterial Static = new SilverPlatingMaterial();
        public override int Hardness => 10;
        public override string Name => @"Alchemal Silver";
        public override int StructurePerInch => 30;
    };
    #endregion

    #region Cold Iron: HP/inch=30, Hardness=10
    /// <summary>Cold Iron (HP/inch=30, Hardness=10)</summary>
    [Serializable]
    public class ColdIronMaterial : MetalMaterial
    {
        public static ColdIronMaterial Static = new ColdIronMaterial();
        public override int Hardness => 10;
        public override string Name => @"Cold Iron";
        public override int StructurePerInch => 30;
    }
    #endregion

    #region Alchemal Gold: HP/inch=30, Hardness=10 (gold-edged steel)
    /// <summary>Alchemal Gold (HP/inch=30, Hardness=10)</summary>
    [Serializable]
    public class GoldPlatingMaterial : MetalMaterial
    {
        public static GoldPlatingMaterial Static = new GoldPlatingMaterial();
        public override int Hardness => 10;
        public override string Name => @"Alchemal Gold";
        public override int StructurePerInch => 30;
    };
    #endregion

}
