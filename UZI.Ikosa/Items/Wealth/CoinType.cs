using System;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Items.Wealth
{
    /// <summary>Base class for coin types</summary>
    [Serializable]
    public class CoinType
    {
        #region construction
        public CoinType(string name, string plural, decimal unitFactor, double weight, PreciousMetal metal)
        {
            _Name = name;
            _Plural = plural;
            _UnitFactor = unitFactor;
            _Weight = weight;
            _Metal = metal;
        }
        #endregion

        #region data
        private readonly string _Name;
        private readonly string _Plural;
        private readonly decimal _UnitFactor;
        private readonly double _Weight;
        private readonly PreciousMetal _Metal;
        #endregion

        public string Name => _Name;
        public string Plural => _Plural;
        public decimal UnitFactor => _UnitFactor;
        public double UnitWeight => _Weight;
        public PreciousMetal PreciousMetal => _Metal;

        public CoinTypeInfo ToCoinTypeInfo()
            => new CoinTypeInfo
            {
                Name = Name,
                PluralName = Plural,
                UnitFactor = UnitFactor,
                UnitWeight = UnitWeight,
                PreciousMetal = PreciousMetal?.Name ?? string.Empty
            };
    }

    /// <summary>Gold Piece (1 unit; 100/#)</summary>
    [Serializable]
    public class GoldPiece : CoinType
    {
        /// <summary>Gold Piece (1 unit; 100/#)</summary>
        public GoldPiece()
            : base(@"gold piece", @"gold pieces", 1m, 0.01d, Gold.Static)
        {
        }

        public readonly static GoldPiece Static = new GoldPiece();
    }

    /// <summary>Silver Piece (0.1 unit; 100/#)</summary>
    [Serializable]
    public class SilverPiece : CoinType
    {
        /// <summary>Silver Piece (0.1 unit; 100/#)</summary>
        public SilverPiece()
            : base(@"silver piece", @"silver pieces", 0.1m, 0.01d, Silver.Static)
        {
        }
        public readonly static SilverPiece Static = new SilverPiece();
    }

    /// <summary>Copper Piece (0.01 unit; 100/#)</summary>
    [Serializable]
    public class CopperPiece : CoinType
    {
        /// <summary>Copper Piece (0.01 unit; 100/#)</summary>
        public CopperPiece()
            : base(@"copper piece", @"copper pieces", 0.01m, 0.01d, Copper.Static)
        {
        }
        public readonly static CopperPiece Static = new CopperPiece();
    }

    /// <summary>Platinum Piece (10 units; 50/#)</summary>
    [Serializable]
    public class PlatinumPiece : CoinType
    {
        /// <summary>Platinum Piece (10 units; 50/#)</summary>
        public PlatinumPiece()
            : base(@"platinum piece", @"platinum pieces", 10m, 0.02d, Platinum.Static)
        {
        }
        public readonly static PlatinumPiece Static = new PlatinumPiece();
    }
}
