using System;

namespace Uzi.Ikosa.Items.Materials
{
    [Serializable]
    public abstract class PreciousMetal : MetalMaterial
    {
        public override int Hardness => 10;
        public override int StructurePerInch => 30;
        public abstract decimal PoundPrice { get; }
    }

    /// <summary>Standard Precious Platinum</summary>
    [Serializable]
    public class Platinum : PreciousMetal
    {
        public override string Name => @"Platinum"; 
        public override decimal PoundPrice => 500m;
        public readonly static Platinum Static = new Platinum();
    }

    /// <summary>Standard Precious Gold</summary>
    [Serializable]
    public class Gold : PreciousMetal
    {
        public override string Name => @"Gold"; 
        public override decimal PoundPrice => 100m;
        public readonly static Gold Static = new Gold();
    }

    /// <summary>Standard Precious Silver</summary>
    [Serializable]
    public class Silver : PreciousMetal
    {
        public override string Name => @"Silver"; 
        public override decimal PoundPrice => 10m;
        public readonly static Silver Static = new Silver();
    }

    /// <summary>Standard Precious Copper</summary>
    [Serializable]
    public class Copper : PreciousMetal
    {
        public override string Name => @"Copper"; 
        public override decimal PoundPrice => 1m;
        public readonly static Copper Static = new Copper();
    }
}
