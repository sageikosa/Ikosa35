using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Wealth
{
    [Serializable]
    public class CoinSet : ItemBase, IActionProvider, IProcessFeedback
    {
        #region construction
        public CoinSet(params CoinCount[] coins)
            : base(@"Coins", Size.Fine)
        {
            _Coins = new List<CoinCount>(coins);
            Price.IsTradeGood = true;
            _IsCounted = false;
            AddIInteractHandler(this);
            CalcPriceSizeWeight();
        }
        #endregion

        #region data
        private List<CoinCount> _Coins;
        private bool _IsCounted;
        #endregion

        #region public CoinCount this[Type coinType] { get; set; }
        public int this[CoinType coinType]
        {
            get => this[coinType?.Name].Count;
            set
            {
                var _coins = this[coinType.Name];
                if (_coins.CoinType != null)
                {
                    _Coins.Remove(_coins);
                }
                if (value > 0)
                {
                    _Coins.Add(new CoinCount { CoinType = coinType, Count = value });
                }
                CalcPriceSizeWeight();
            }
        }
        #endregion

        /// <summary>If not found, returns the default which is null and </summary>
        public CoinCount this[string coinTypeName]
            => _Coins.FirstOrDefault(_c => string.Equals(_c.CoinType.Name, coinTypeName));

        public string CoinTypeName
        {
            get
            {
                var _dc = _Coins.Select(_c => _c.CoinType.Name).Distinct().ToList();
                if (_dc.Count == 1)
                    return _dc.FirstOrDefault();
                else
                    return @"-Mixed-";
            }
        }

        public int Count => _Coins.Sum(_c => _c.Count);

        public IEnumerable<CoinCount> Coins => _Coins.Select(_c => _c);
        public bool IsCounted { get => _IsCounted; set => _IsCounted = value; }

        /// <summary>
        /// Get a CoinCount that has at most the specified number of coins of the specified type.
        /// Coins are removed from the CoinSet to satisfy the demand.  
        /// If the set is emptied, it is destroyed.
        /// </summary>
        public CoinCount GetCoins(string coinType, int count)
        {
            var _coins = this[coinType];
            if (_coins.CoinType == null)
            {
                return _coins;
            }
            else if (_coins.Count <= count)
            {
                // return it all
                _Coins.Remove(_coins);
                if (_Coins.Count == 0)
                {
                    // last count in the set
                    StructurePoints = 0;
                }
                return _coins;
            }

            // subtract amount getting from the set
            this[_coins.CoinType] = _coins.Count - count;

            // and return the count getting
            return new CoinCount { CoinType = _coins.CoinType, Count = count };
        }

        public void AddCoins(CoinCount coins)
        {
            if (coins.CoinType != null)
            {
                // if coinType not found, this will be 0
                var _amt = this[coins.CoinType];
                this[coins.CoinType] = _amt + Math.Max(coins.Count, 0);
            }
        }

        #region private void CalcPriceSizeWeight()
        private void CalcPriceSizeWeight()
        {
            if (_Coins.Count == 0)
            {
                Price.CorePrice = 0m;
                ItemSizer.NaturalSize = Size.Fine;
                BaseWeight = 0d;
                return;
            }
            Price.CorePrice = _Coins.Sum(_c => _c.CoinType.UnitFactor * _c.Count);
            var _newWeight = _Coins.Sum(_c => _c.CoinType.UnitWeight * _c.Count);
            if (_newWeight <= 1) // 50 GP
            {
                ItemSizer.NaturalSize = Size.Fine;
            }
            else if (_newWeight <= 8) // 400 GP
            {
                ItemSizer.NaturalSize = Size.Miniature;
            }
            else if (_newWeight <= 64) // 3,200 GP
            {
                ItemSizer.NaturalSize = Size.Tiny;
            }
            else if (_newWeight <= 512) // 25,600 GP
            {
                ItemSizer.NaturalSize = Size.Small;
            }
            else if (_newWeight <= 4096) // 204,800 GP
            {
                ItemSizer.NaturalSize = Size.Medium;
            }
            else if (_newWeight <= 32768) // 1,638,400 GP
            {
                ItemSizer.NaturalSize = Size.Large;
            }
            else if (_newWeight <= 262144) // 13,107,200 GP
            {
                ItemSizer.NaturalSize = Size.Huge;
            }
            else if (_newWeight <= 2097152) // 104,857,600 GP
            {
                ItemSizer.NaturalSize = Size.Gigantic;
            }
            else if (_newWeight <= 16777216) // 838,860,800 GP
            {
                ItemSizer.NaturalSize = Size.Colossal;
            }
            BaseWeight = _newWeight;
        }
        #endregion

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // TODO: count coins action (one time deal)
            // TODO: remove coins from set
            // TODO: add coins to set
            // TODO: merge/divide coins
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);

        #endregion

        #region IInteractHandler Members

        void IInteractHandler.HandleInteraction(Interaction workSet)
        {
        }

        IEnumerable<Type> IInteractHandler.GetInteractionTypes()
        {
            yield return typeof(GetInfoData);
            yield break;
        }

        bool IInteractHandler.LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            if (typeof(GetInfoData).Equals(interactType))
            {
                // TODO: negotiation with other stuff?
                if (existingHandler is GetInfoDataHandler)
                    return true;
            }
            return false;
        }

        #endregion

        #region IProcessFeedback Members

        void IProcessFeedback.ProcessFeedback(Interaction workSet)
        {
            if ((workSet != null) && (workSet.InteractData is GetInfoData))
            {
                // provide iInfos, but only to creatures that match the critter list
                if ((Possessor != null) && (workSet.Actor != null)
                    && (workSet.Actor.ID == Possessor.ID) && IsCounted
                    && (workSet.Feedback != null))
                {
                    var _infoBack = workSet.Feedback.OfType<InfoFeedback>().FirstOrDefault();
                    if (_infoBack != null)
                    {
                        var _info = _infoBack.Information as ObjectInfo;
                        if (_info != null)
                        {
                            var _informs = (from _c in _Coins
                                            select string.Format(@"{0}({1}): {2}",
                                            _c.CoinType.Plural, _c.CoinType.PreciousMetal.Name, _c.Count)).ToArray();
                            _info.AdjunctInfos = _info.AdjunctInfos.Union(new Description(Name, _informs).ToEnumerable()).ToArray();
                        }
                    }
                }
            }
        }

        #endregion

        protected override string ClassIconKey => @"coins";
    }

    [Serializable]
    public struct CoinCount
    {
        public CoinType CoinType { get; set; }
        public int Count { get; set; }
    }
}
