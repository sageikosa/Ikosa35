using System;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Items.Materials
{
    [Serializable]
    public abstract class GemMaterial : StoneMaterial
    {
        public abstract Roller ValueRandomizer { get; }
        public abstract string DisplayName { get; }
        public abstract string AccentColor { get; }
        public virtual bool IsEdged => false;
    }

    // -- Ornamental
    [Serializable]
    public class OrnamentalGemMaterial : GemMaterial
    {
        public override Roller ValueRandomizer => new ComplexDiceRoller(@"4d4");
        public override string Name => @"Ornamental Gem";
        public override string DisplayName => @"Bauble";
        public override string AccentColor => null;
        public new static readonly OrnamentalGemMaterial Static = new OrnamentalGemMaterial();
    }

    // -- Semi-Precious
    [Serializable]
    public class SemiPreciousGemMaterial : GemMaterial
    {
        public override Roller ValueRandomizer => new ComplexDiceRoller(@"2d4*10");
        public override string Name => @"Semi-Precious Gem";
        public override string DisplayName => @"Bauble";
        public override string AccentColor => null;
        public new static readonly SemiPreciousGemMaterial Static = new SemiPreciousGemMaterial();
    }

    [Serializable]
    public class OnyxMaterial : SemiPreciousGemMaterial
    {
        public override string Name => @"Onyx";
        public override string DisplayName => @"Red Banded Bauble";
        public override string AccentColor => @"Red";
        public new static readonly OnyxMaterial Static = new OnyxMaterial();
    }

    [Serializable]
    public class BlackOnyxMaterial : OnyxMaterial
    {
        public override string Name => @"Black Onyx";
        public override string DisplayName => @"Black Banded Bauble";
        public override string AccentColor => @"Black";
        public new static readonly BlackOnyxMaterial Static = new BlackOnyxMaterial();
    }

    // -- Fancy
    [Serializable]
    public class FancyGemMaterial : GemMaterial
    {
        public override Roller ValueRandomizer => new ComplexDiceRoller(@"4d4*10");
        public override string Name => @"Fancy Gem";
        public override string DisplayName => @"Gem";
        public override string AccentColor => null;
        public new static readonly FancyGemMaterial Static = new FancyGemMaterial();
    }

    [Serializable]
    public class AmberMaterial : FancyGemMaterial
    {
        public override string Name => @"Amber";
        public override string DisplayName => @"Orange Gem";
        public override string AccentColor => @"Orange";
        public new static readonly AmberMaterial Static = new AmberMaterial();
    }

    [Serializable]
    public class JadeMaterial : FancyGemMaterial
    {
        public override string Name => @"Jade";
        public override string DisplayName => @"Green Gem";
        public override string AccentColor => @"LimeGreen";
        public new static readonly JadeMaterial Static = new JadeMaterial();
    }

    [Serializable]
    public class PearlMaterial : FancyGemMaterial
    {
        public override int Hardness => 2;
        public override int StructurePerInch => 5;
        public override string Name => @"Pearl";
        public override string DisplayName => @"White Gem";
        public override string AccentColor => @"White";
        public new static readonly PearlMaterial Static = new PearlMaterial();
    }

    [Serializable]
    public class BlackPearlMaterial : PearlMaterial // NOTE: treated as a pearl for material purposes
    {
        public override Roller ValueRandomizer => new ComplexDiceRoller(@"2d4*100");
        public override string Name => @"Black Pearl";
        public override string DisplayName => @"Black Gem";
        public override string AccentColor => @"Black";
        public new static readonly BlackPearlMaterial Static = new BlackPearlMaterial();
    }

    // -- Precious
    [Serializable]
    public class PreciousGemMaterial : GemMaterial
    {
        public override Roller ValueRandomizer => new ComplexDiceRoller(@"2d4*100");
        public override string Name => @"Precious Gem";
        public override string DisplayName => @"Gem";
        public override string AccentColor => null;
        public override bool IsEdged => true;
        public new static readonly PreciousGemMaterial Static = new PreciousGemMaterial();
    }

    // -- Exquisite
    [Serializable]
    public class ExquisiteGemMaterial : GemMaterial
    {
        public override Roller ValueRandomizer => new ComplexDiceRoller(@"4d4*100");
        public override string Name => @"Exquisite Gem";
        public override string DisplayName => @"Gem";
        public override string AccentColor => null;
        public override bool IsEdged => true;
        public new static readonly ExquisiteGemMaterial Static = new ExquisiteGemMaterial();
    }

    [Serializable]
    public class RubyMaterial : ExquisiteGemMaterial
    {
        public override string Name => @"Ruby";
        public override string DisplayName => @"Red Gem";
        public override string AccentColor => @"Red";
        public override bool IsEdged => true;
        public new static readonly RubyMaterial Static = new RubyMaterial();
    }

    [Serializable]
    public class OpalMaterial : ExquisiteGemMaterial
    {
        public override string Name { get { return @"Opal"; } }
        public override string DisplayName => @"Colorful Gem";
        public override string AccentColor => @"Violet";
        public override bool IsEdged => true;
        public new static readonly OpalMaterial Static = new OpalMaterial();
    }

    // -- Exalted
    [Serializable]
    public class ExaltedGemMaterial : GemMaterial
    {
        public override Roller ValueRandomizer => new ComplexDiceRoller(@"2d4*1000");
        public override string Name => @"Exalted Gem";
        public override string DisplayName => @"Gem";
        public override string AccentColor => null;
        public override bool IsEdged => true;
        public new static readonly ExaltedGemMaterial Static = new ExaltedGemMaterial();
    }

    [Serializable]
    public class DiamondMaterial : ExaltedGemMaterial
    {
        public override string Name => @"Diamond";
        public override string DisplayName => @"Clear Gem";
        public override string AccentColor => @"#80F5F5F5";
        public override bool IsEdged => true;
        public new static readonly DiamondMaterial Static = new DiamondMaterial();
    }

    [Serializable]
    public class JacinthMaterial : ExaltedGemMaterial
    {
        public override string Name => @"Jacinth";
        public override string DisplayName => @"Red-Orange Gem";
        public override string AccentColor => @"OrangeRed";
        public override bool IsEdged => true;
        public new static readonly JacinthMaterial Static = new JacinthMaterial();
    }
}
