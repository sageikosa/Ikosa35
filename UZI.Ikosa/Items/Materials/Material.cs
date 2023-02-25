using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Materials
{
    /// <summary>
    /// Item Material
    /// </summary>
    [Serializable]
    public abstract class Material : IEquatable<Material>
    {
        public abstract string Name { get; }
        public abstract int StructurePerInch { get; }
        public abstract int Hardness { get; }
        public abstract string SoundQuality { get; }
        public MaterialInfo ToMaterialInfo()
        {
            return new MaterialInfo
            {
                Name = Name,
                StructurePerInch = StructurePerInch,
                Hardness = Hardness
            };
        }

        #region IEquatable<Material> Members

        public virtual bool Equals(Material other)
        {
            return (other.GetType() == GetType());
        }

        #endregion
    };

    [Serializable]
    public abstract class GenericMaterial : Material
    {
        #region Constructor
        public GenericMaterial(string name, int hpInch, int hardness)
        {
            _Name = name;
            _StructureInch = hpInch;
            _Hardness = hardness;
        }
        #endregion

        #region private data
        protected string _Name;
        protected int _StructureInch;
        protected int _Hardness;
        #endregion

        public override string Name { get { return _Name; } }
        public override int StructurePerInch { get { return _StructureInch; } }
        public override int Hardness { get { return _Hardness; } }
    }

    [Serializable]
    public class VoidMaterial : Material
    {
        public readonly static VoidMaterial Static = new VoidMaterial();
        public override int Hardness { get { return 0; } }
        public override string Name { get { return @"Void"; } }
        public override int StructurePerInch { get { return 0; } }
        public override string SoundQuality => @"empty";
    }

    #region Paper:  HP/inch=2, Hardness=0
    /// <summary>Paper (HP/inch=2, Hardness=0)</summary>
    [Serializable]
    public class PaperMaterial : Material
    {
        public readonly static PaperMaterial Static = new PaperMaterial();
        public override int Hardness { get { return 0; } }
        public override string Name { get { return @"Paper"; } }
        public override int StructurePerInch { get { return 2; } }
        public override string SoundQuality => @"flapping";
    };
    #endregion

    #region Cloth:  HP/inch=2, Hardness=0
    /// <summary>Cloth (HP/inch=2, Hardness=0)</summary>
    [Serializable]
    public class ClothMaterial : Material
    {
        public readonly static ClothMaterial Static = new ClothMaterial();
        public override int Hardness { get { return 0; } }
        public override string Name { get { return @"Cloth"; } }
        public override int StructurePerInch { get { return 2; } }
        public override string SoundQuality => @"taut";
    };
    #endregion

    #region Rope:  HP/inch=2, Hardness=0
    /// <summary>Rope (HP/inch=2, Hardness=0)</summary>
    [Serializable]
    public class RopeMaterial : Material
    {
        public readonly static RopeMaterial Static = new RopeMaterial();
        public override int Hardness { get { return 0; } }
        public override string Name { get { return @"Rope"; } }
        public override int StructurePerInch { get { return 2; } }
        public override string SoundQuality => @"taut";
    };
    #endregion

    #region Glass:  HP/inch=1, Hardness=1
    /// <summary>
    /// Glass (HP/inch=1, Hardness=1)
    /// </summary>
    [Serializable]
    public class GlassMaterial : Material
    {
        public readonly static GlassMaterial Static = new GlassMaterial();
        public override int Hardness { get { return 1; } }
        public override string Name { get { return @"Glass"; } }
        public override int StructurePerInch { get { return 1; } }
        public override string SoundQuality => @"ringing";
    };
    #endregion

    #region Ice:  HP/inch=3, Hardness=0
    /// <summary>
    /// Ice (HP/inch=3, Hardness=0)
    /// </summary>
    [Serializable]
    public class IceMaterial : Material
    {
        public readonly static IceMaterial Static = new IceMaterial();
        public override int Hardness { get { return 0; } }
        public override string Name { get { return @"Ice"; } }
        public override int StructurePerInch { get { return 3; } }
        public override string SoundQuality => @"ringing";
    };
    #endregion

    #region Leather:  HP/inch=5, Hardness=2
    /// <summary>
    /// Leather (HP/inch=5, Hardness=2)
    /// </summary>
    [Serializable]
    public class LeatherMaterial : Material
    {
        public readonly static LeatherMaterial Static = new LeatherMaterial();
        public override int Hardness { get { return 2; } }
        public override string Name { get { return @"Leather"; } }
        public override int StructurePerInch { get { return 5; } }
        public override string SoundQuality => @"soft";
    };
    #endregion

    #region Hide:  HP/inch=5, Hardness=2
    /// <summary>
    /// Hide (HP/inch=5, Hardness=2)
    /// </summary>
    [Serializable]
    public class HideMaterial : Material
    {
        public readonly static HideMaterial Static = new HideMaterial();
        public override int Hardness { get { return 2; } }
        public override string Name { get { return @"Hide"; } }
        public override int StructurePerInch { get { return 5; } }
        public override string SoundQuality => @"soft";
    };
    #endregion

    [Serializable]
    public class ExoskeletonMaterial : HideMaterial
    {
        public readonly static new ExoskeletonMaterial Static = new ExoskeletonMaterial();
        public override int Hardness => 5;
        public override string Name => @"Exoskeletal";
        public override int StructurePerInch => 8;
        public override string SoundQuality => @"rigid";
    }

    #region Wood:  HP/inch=10, Hardness=5
    /// <summary>Wood (HP/inch=10, Hardness=5)</summary>
    [Serializable]
    public class WoodMaterial : Material
    {
        public readonly static WoodMaterial Static = new WoodMaterial();
        public override int Hardness { get { return 5; } }
        public override string Name { get { return @"Wood"; } }
        public override int StructurePerInch { get { return 10; } }
        public override string SoundQuality => @"soft";
    };
    #endregion

    #region SoftPlantMaterial:  HP/inch=2, Hardness=0
    /// <summary>Wood (HP/inch=10, Hardness=5)</summary>
    [Serializable]
    public class SoftPlantMaterial : Material
    {
        public readonly static SoftPlantMaterial Static = new SoftPlantMaterial();
        public override int Hardness { get { return 0; } }
        public override string Name { get { return @"Soft Plant"; } }
        public override int StructurePerInch { get { return 2; } }
        public override string SoundQuality => @"soft";
    };
    #endregion

    /// <summary>Bone (HP/inch=10, Hardness=6)</summary>
    [Serializable]
    public class BoneMaterial : Material
    {
        public readonly static BoneMaterial Static = new BoneMaterial();
        public override int Hardness { get { return 6; } }
        public override string Name { get { return @"Bone"; } }
        public override int StructurePerInch { get { return 10; } }
        public override string SoundQuality => @"crisp";
    }

    /// <summary>Ooze (HP/inch=2, Hardness=1)</summary>
    [Serializable]
    public class OozeMaterial : Material
    {
        public readonly static OozeMaterial Static = new OozeMaterial();
        public override int Hardness { get { return 1; } }
        public override string Name { get { return @"Ooze"; } }
        public override int StructurePerInch { get { return 2; } }
        public override string SoundQuality => @"squishy";
    }

    [Serializable]
    public class ElementalMaterial : Material
    {
        public readonly static ElementalMaterial Static = new ElementalMaterial();
        public override int Hardness => 2;
        public override string Name => @"Elemental";
        public override int StructurePerInch => 5;
        public override string SoundQuality => @"howling";
    }
}
