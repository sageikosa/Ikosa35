using System;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;
using Newtonsoft.Json;

namespace Uzi.Ikosa
{
    /// <summary>
    /// <para>Size used to determine bonuses, penalties, carrying capacity and movement restrictions.</para>
    /// <para>Colossal &gt; Gigantic &gt; Huge &gt; Large &gt; Medium &gt; Small &gt; Tiny &gt; Miniature &gt; Fine</para>
    /// </summary>
    [Serializable]
    public class Size
    {
        internal Size(int order, int sizeDelta, int opposedDelta)
        {
            Order = order;
            SizeDelta = new ConstDelta(sizeDelta, typeof(Size), string.Format(@"{0} Size", Name));
            OpposedDelta = new ConstDelta(opposedDelta, typeof(Size), string.Format(@"{0} Size", Name));
            HideDelta = new ConstDelta(0 - opposedDelta, typeof(Size), string.Format(@"{0} Size", Name));
        }

        #region public string Name { get; }
        public string Name
        {
            get
            {
                switch (Order)
                {
                    case -4:
                        return @"Fine";
                    case -3:
                        return @"Miniature";
                    case -2:
                        return @"Tiny";
                    case -1:
                        return @"Small";
                    case 0:
                        return @"Medium";
                    case 1:
                        return @"Large";
                    case 2:
                        return @"Huge";
                    case 3:
                        return @"Gigantic";
                    case 4:
                        return @"Colossal";
                }
                return @"???";
            }
        }
        #endregion

        /// <summary>Ordinal number indicating the size step from medium (-4..0..+4)</summary>
        public int Order { get; private set; }

        /// <summary>For ATK and AR</summary>
        public readonly IModifier SizeDelta;

        /// <summary>Grapple etc. deltas</summary>
        public readonly IModifier OpposedDelta;

        /// <summary>Negative grapple deltas</summary>
        public readonly IModifier HideDelta;

        public SizeInfo ToSizeInfo()
            => new SizeInfo
            {
                Name = Name,
                Order = Order
            };

        #region public GeometricSize CubeSize()
        public GeometricSize CubeSize()
        {
            switch (Order)
            {
                case -4:
                    return new GeometricSize(0.1, 0.1, 0.1);
                case -3:
                    return new GeometricSize(0.2, 0.2, 0.2);
                case -2:
                    // NOTE: altered from expected cube size based on space to accomodate model scaling
                    return new GeometricSize(0.35, 0.35, 0.35);
                case -1:
                    // NOTE: altered from expected cube size based on space to accomodate model scaling
                    return new GeometricSize(0.6, 0.6, 0.6);
                case 0:
                    return new GeometricSize(1, 1, 1);
                case 1:
                    return new GeometricSize(2, 2, 2);
                case 2:
                    return new GeometricSize(3, 3, 3);
                case 3:
                    return new GeometricSize(4, 4, 4);
                default:
                    return new GeometricSize(6, 6, 6);
            }
        }
        #endregion

        #region public double BaseCarryingFactor { get; }
        /// <summary>Multiplier for strength carrying capacity (biped)</summary>
        public double BaseCarryingFactor
        {
            get
            {
                switch (Order)
                {
                    case -4:
                        return 0.125;
                    case -3:
                        return 0.25;
                    case -2:
                        return 0.5;
                    case -1:
                        return 0.75;
                    case 0:
                        return 1;
                    case 1:
                        return 2;
                    case 2:
                        return 4;
                    case 3:
                        return 8;
                    case 4:
                        return 16;
                }
                return 1;
            }
        }
        #endregion

        #region public double ExtraCarryingFactor { get; }
        /// <summary>Multiplier for strength carrying capacity (quadruped+)</summary>
        public double ExtraCarryingFactor
        {
            get
            {
                switch (Order)
                {
                    case -4:
                        return 0.25;
                    case -3:
                        return 0.5;
                    case -2:
                        return 0.75;
                    case -1:
                        return 1;
                    case 0:
                        return 1.5;
                    case 1:
                        return 3;
                    case 2:
                        return 6;
                    case 3:
                        return 12;
                    case 4:
                        return 24;
                }
                return 1.5;
            }
        }
        #endregion

        #region public double ItemWeightFactor { get; }
        /// <summary>Multiplier for item weight and structure (compared to medium) for a creature this size.</summary>
        public double ItemWeightFactor
        {
            get
            {
                switch (Order)
                {
                    case -4:
                        return 0.0625d;
                    case -3:
                        return 0.125d;
                    case -2:
                        return 0.25d;
                    case -1:
                        return 0.5d;
                    case 0:
                        return 1d;
                    case 1:
                        return 2d;
                    case 2:
                        return 4d;
                    case 3:
                        return 8d;
                    case 4:
                        return 16d;
                }
                return 1;
            }
        }
        #endregion

        #region public decimal ItemCostFactor { get; }
        /// <summary>Multiplier for item cost (compared to medium) for a creature this size.</summary>
        public decimal ItemCostFactor
        {
            get
            {
                switch (Order)
                {
                    case -4:
                        return 0.4m;
                    case -3:
                        return 0.45m;
                    case -2:
                        return 0.5m;
                    case -1:
                    case 0:
                        return 1m;
                    case 1:
                        return 2m;
                    case 2:
                        return 4m;
                    case 3:
                        return 8m;
                    case 4:
                        return 16m;
                }
                return 1m;
            }
        }
        #endregion

        #region public decimal ArmorBonusFactor { get; }
        /// <summary>Multiplier for armor bonus (compared to medium) for a creature this size.</summary>
        public decimal ArmorBonusFactor
        {
            get
            {
                switch (Order)
                {
                    case -4:
                    case -3:
                    case -2:
                        return 0.5m;
                    default:
                        return 1m;
                }
            }
        }
        #endregion

        #region public Size OffsetSize(int sizeOffset)
        public Size OffsetSize(int sizeOffset)
        {
            int _newSize = Order + sizeOffset;
            if (_newSize < -4)
            {
                _newSize = -4;
            }

            switch (_newSize)
            {
                case -4:
                    return Size.Fine;
                case -3:
                    return Size.Miniature;
                case -2:
                    return Size.Tiny;
                case -1:
                    return Size.Small;
                case 0:
                    return Size.Medium;
                case 1:
                    return Size.Large;
                case 2:
                    return Size.Huge;
                case 3:
                    return Size.Gigantic;
                default:
                    return Size.Colossal;
            }
        }
        #endregion

        #region public static Size SmallerSize(Size currentSize)
        public static Size SmallerSize(Size currentSize)
        {
            switch (currentSize.Order)
            {
                case -3:
                    return Size.Fine;
                case -2:
                    return Size.Miniature;
                case -1:
                    return Size.Tiny;
                case 0:
                    return Size.Small;
                case 1:
                    return Size.Medium;
                case 2:
                    return Size.Large;
                case 3:
                    return Size.Huge;
                case 4:
                    return Size.Gigantic;
                default:
                    return currentSize;
            }
        }
        #endregion

        #region public static Size LargerSize(Size currentSize)
        public static Size LargerSize(Size currentSize)
        {
            switch (currentSize.Order)
            {
                case -4:
                    return Size.Miniature;
                case -3:
                    return Size.Tiny;
                case -2:
                    return Size.Small;
                case -1:
                    return Size.Medium;
                case 0:
                    return Size.Large;
                case 1:
                    return Size.Huge;
                case 2:
                    return Size.Gigantic;
                case 3:
                    return Size.Colossal;
                default:
                    return currentSize;
            }
        }
        #endregion

        /// <summary>ATK/AR = 8, Grapple = -16, Order = -4</summary>
        [NonSerialized, JsonIgnore]
        public static readonly Size Fine = new Size(-4, 8, -16);
        /// <summary>ATK/AR = 4, Grapple = -12, Order = -3</summary>
        [NonSerialized, JsonIgnore]
        public static readonly Size Miniature = new Size(-3, 4, -12);
        /// <summary>ATK/AR = 2, Grapple = -8, Order = -2</summary>
        [NonSerialized, JsonIgnore]
        public static readonly Size Tiny = new Size(-2, 2, -8);
        /// <summary>ATK/AR = 1, Grapple = -4, Order = -1</summary>
        [NonSerialized, JsonIgnore]
        public static readonly Size Small = new Size(-1, 1, -4);
        /// <summary>ATK/AR = 0, Grapple = 0, Order = 0</summary>
        [NonSerialized, JsonIgnore]
        public static readonly Size Medium = new Size(0, 0, 0);
        /// <summary>ATK/AR = -1, Grapple = 4, Order = 1</summary>
        [NonSerialized, JsonIgnore]
        public static readonly Size Large = new Size(1, -1, 4);
        /// <summary>ATK/AR = -2, Grapple = 8, Order = 2</summary>
        [NonSerialized, JsonIgnore]
        public static readonly Size Huge = new Size(2, -2, 8);
        /// <summary>ATK/AR = -4, Grapple = 12, Order = 3</summary>
        [NonSerialized, JsonIgnore]
        public static readonly Size Gigantic = new Size(3, -4, 12);
        /// <summary>ATK/AR = -8, Grapple = 16, Order = 4</summary>
        [NonSerialized, JsonIgnore]
        public static readonly Size Colossal = new Size(4, -8, 16);

        public static Size FromSizeOrder(int sizeOrder)
            => Size.Medium.OffsetSize(sizeOrder);
    }
}
