using System;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa
{
    /// <summary>
    /// Controls item size by expected creature size (starts as Medium).  
    /// Also, manages cost factors for size.
    /// </summary>
    [Serializable]
    public class ItemSizer : ObjectSizer
    {
        #region construction
        public ItemSizer(Size size, IItemBase coreObj)
            : base(size, coreObj)
        {
            _ExpectedCreatureSize = Size.Medium;
            _NaturalOffset = size.Order;
            _BaseSize = NaturalSize;
        }

        public ItemSizer(Size size, IItemBase coreObj, IObjectContainer container)
            : base(size, coreObj, container)
        {
            _ExpectedCreatureSize = Size.Medium;
            _NaturalOffset = size.Order;
            _BaseSize = NaturalSize;
        }
        #endregion

        #region private data
        protected Size _ExpectedCreatureSize;
        private int _NaturalOffset;
        private Size _BaseSize;
        #endregion

        /// <summary>Defined size (relative to a medium creature)</summary>
        public Size BaseSize => _BaseSize;

        #region public override Size NaturalSize { get; set; }
        public override Size NaturalSize
        {
            get { return base.NaturalSize; }
            set
            {
                // change values without signaling, mainly because this is supposed to be the "true" size
                _NaturalSize = value;
                _NaturalOffset = ExpectedCreatureSize.Order + value.Order;

                // base order change means we explicitly redefined the natural size of the object
                _BaseSize = NaturalSize;

                // projected size did change
                _Size = _NaturalSize.OffsetSize(SizeOffset.EffectiveValue);

                // notify of size changes (but no side effects within the sizer)
                DoPropertyChanged(@"Size");
                DoPropertyChanged(@"NaturalSize");
                DoPropertyChanged(@"BaseSize");
            }
        }
        #endregion

        #region public Size ExpectedCreatureSize { get; set; }
        /// <summary>
        /// Gets or sets the size of the creature for whom the item is originally sized.
        /// This is the size for whom the item is "manufactured".  
        /// All items are originally sized for medium creatures.
        /// </summary>
        public Size ExpectedCreatureSize
        {
            get { return _ExpectedCreatureSize; }
            set
            {
                if ((value != null) && (value.Order != _ExpectedCreatureSize.Order))
                {
                    // cancel old cost factor of item...
                    var _item = _Core as IItemBase;
                    if (!_item.Price.IsTradeGood)
                        _item.Price.SizePriceFactor /= ExpectedCreatureSize.ItemCostFactor;
                    var _strucBase = Math.Max(1d, _item.MaxStructurePoints.BaseDoubleValue / NaturalSize.ItemWeightFactor);

                    // adjust creature expectations
                    var _change = value.Order - _ExpectedCreatureSize.Order;
                    _ExpectedCreatureSize = value;

                    // NOTE: use simple x2, /2 to keep altered weight in line with strength capacity for size changes
                    if (_change != 0)
                    {
                        // since items are all based on relative sizing, must adjust max load and tare weight when expected size changes
                        var _factor = Math.Pow(2, _change);
                        foreach (var _c in Containers)
                        {
                            _c.MaximumLoadWeight *= _factor;
                            _c.TareWeight *= _factor;
                        }
                        if (_Core is IOpenable)
                        {
                            var _open = _Core as IOpenable;
                            _open.OpenWeight *= _factor;
                        }
                        if (_Core is IAmmunitionBundle)
                        {
                            // resize all ammunition prototypes...
                            foreach (var _ammoSet in (_Core as IAmmunitionBundle).AmmoSets)
                            {
                                _ammoSet.Ammunition.ItemSizer.ExpectedCreatureSize = value;
                            }
                        }
                    }

                    // "base" natural size = creature size + natural offset
                    base.NaturalSize = ExpectedCreatureSize.OffsetSize(_NaturalOffset);

                    // factor in new cost factor of item...
                    if (!_item.Price.IsTradeGood)
                        _item.Price.SizePriceFactor *= ExpectedCreatureSize.ItemCostFactor;
                    _item.MaxStructurePoints.BaseDoubleValue = Math.Max(1d, _strucBase * NaturalSize.ItemWeightFactor);

                    DoPropertyChanged(@"ExpectedCreatureSize");
                    DoPropertyChanged(@"EffectiveCreatureSize");
                }
            }
        }
        #endregion

        protected override void OnSizeChange()
        {
            // if size changes, so does the effective creature size
            base.OnSizeChange();
            var _item = _Core as IItemBase;
            _item.Weight = 0;
            DoPropertyChanged(@"EffectiveCreatureSize");
        }

        /// <summary>
        /// Gets the size of the creature that can currently use the item normally
        /// </summary>
        public virtual Size EffectiveCreatureSize
            => ExpectedCreatureSize.OffsetSize(SizeOffset.EffectiveValue);
    }
}
