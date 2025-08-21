using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class Price : INotifyPropertyChanged, IProcessFeedback
    {
        public Price(ItemBase item, bool tradeGood, decimal corePrice)
        {
            _Item = item;
            _CorePrice = corePrice;
            _Trade = tradeGood;
        }

        #region private data
        private ItemBase _Item = null;
        private bool _Trade;
        private decimal _CorePrice = 0;
        private decimal _CoreExtraPrice = 0;
        private decimal _CorePriceFactor = 1;
        private decimal _BaseExtraPrice = 0;
        private decimal _BaseItemExtraPrice = 0;
        #endregion

        public void DoPriceChange()
        {
            DoPropertyChanged(@"BasePrice");
            DoPropertyChanged(@"SellPrice");
        }

        /// <summary>If true, price does not adjust by size, and sell price is equal to base price</summary>
        public bool IsTradeGood
        {
            get { return _Trade; }
            set
            {
                _Trade = value;
                DoPropertyChanged(@"IsTradeGood");
                DoPropertyChanged(@"SellPrice");
            }
        }

        #region public decimal CorePrice { get; set; }
        public decimal CorePrice
        {
            get { return _CorePrice; }
            set
            {
                if (value > 0)
                {
                    _CorePrice = value;
                    DoPropertyChanged(@"CorePrice");
                    DoPriceChange();
                }
            }
        }
        #endregion

        #region public decimal CoreExtraPrice { get; set; }
        /// <summary>Price (typically for material) at "normal" size</summary>
        public decimal CoreExtraPrice
        {
            get { return _CoreExtraPrice; }
            set
            {
                _CoreExtraPrice = value;
                DoPropertyChanged(@"CoreExtraPrice");
                DoPriceChange();
            }
        }
        #endregion

        #region public decimal SizePriceFactor { get; set; }
        /// <summary>Usually determined by item size relative to "normal"</summary>
        public decimal SizePriceFactor
        {
            get { return _CorePriceFactor; }
            set
            {
                if (_CorePriceFactor > 0)
                {
                    _CorePriceFactor = value;
                    DoPropertyChanged(@"SizePriceFactor");
                    DoPriceChange();
                }
            }
        }
        #endregion

        /// <summary>Extra pricing controlled by the item</summary>
        public decimal BaseItemExtraPrice
        {
            get { return _BaseItemExtraPrice; }
            set
            {
                _BaseItemExtraPrice = value;
                DoPriceChange();
            }
        }

        #region public void DoCalcAugmentationPrice()
        public void DoCalcAugmentationPrice()
        {
            var _costs = _Item.Adjuncts.OfType<IAugmentationCost>()
                .OrderByDescending(_c => _c.StandardCost).ToList();
            if (_costs.Any())
            {
                if (_Item is ISlottedItem)
                {
                    // most expensive power
                    _BaseExtraPrice = _costs.Take(1).Sum(_c => _c.StandardCost * (_c.Affinity ? 1.0m : 1.5m));
                    if (_costs.Count > 1)
                    {
                        // additional (lower priced) powers cost more per power...
                        _BaseExtraPrice += _costs.Skip(1).Sum(_c => _c.StandardCost * 1.5m * (_c.Affinity ? 1.0m : 1.5m));
                    }
                }
                else
                {
                    // NOTE: everything here costs double since it is an unslotted item...
                    _BaseExtraPrice = _costs.Where(_c => _c.SimilarityKey == null).Sum(_c => _c.StandardCost * 2m);

                    // things in similarity groups
                    foreach (var _group in (from _c in _costs
                                            where _c.SimilarityKey != null
                                            group _c by _c.SimilarityKey into _similar
                                            select new
                                            {
                                                Key = _similar.Key,
                                                Ranked = _similar.OrderByDescending(_s => _s.StandardCost).ToList()
                                            }).ToList())
                    {
                        // effectively 1.0m * cost * unslotted
                        _BaseExtraPrice += _group.Ranked.First().StandardCost * 2m;
                        if (_group.Ranked.Count > 1)
                        {
                            // effectively 0.75m * cost * unslotted
                            _BaseExtraPrice += _group.Ranked.Skip(1).First().StandardCost * 1.5m;
                        }
                        if (_group.Ranked.Count > 2)
                        {
                            // effectively 0.5m * cost * unslotted
                            _BaseExtraPrice += _group.Ranked.Skip(2).Sum(_s => _s.StandardCost);
                        }
                    }
                }
            }
            else
            {
                // no augmentation costs
                _BaseExtraPrice = 0m;
            }

            DoPropertyChanged(@"AugmentationPrice");
            DoPriceChange();
        }
        #endregion

        /// <summary>Augmentation price</summary>
        public decimal AugmentationPrice => _BaseExtraPrice;

        /// <summary>Total Calculated Base Prices In Gold Pieces</summary>
        public virtual decimal BasePrice
            => (_CorePrice + _CoreExtraPrice) * SizePriceFactor + AugmentationPrice + BaseItemExtraPrice;

        /// <summary>Half BasePrice, Full BasePrice for trade good</summary>
        public virtual decimal SellPrice
            => IsTradeGood ? BasePrice : (BasePrice / 2m);

        #region INotifyPropertyChanged Members
        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        /// <summary>Allow the PropertyChanged event to be invoked from derived classes</summary>
        protected void DoPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propName));
            }

            if ((_Item != null) && (_Item.CreaturePossessor != null))
            {
                (_Item.CreaturePossessor.Possessions as IkosaPossessions).DoChangePrice();
            }
        }

        // feedback processing

        public void ProcessFeedback(Interaction workSet)
        {
            if (((workSet?.InteractData as AddAdjunctData)?.Adjunct is IAugmentationCost)
                || ((workSet?.InteractData as RemoveAdjunctData)?.Adjunct is IAugmentationCost))
            {
                DoCalcAugmentationPrice();
            }
        }

        public void HandleInteraction(Interaction workSet)
        {
            // no operation
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData);
            yield return typeof(RemoveAdjunctData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // last feedback processor
            if (typeof(AddAdjunctData).Equals(interactType))
            {
                return true;
            }
            else if (typeof(RemoveAdjunctData).Equals(interactType))
            {
                return true;
            }

            return false;
        }
    }
}
