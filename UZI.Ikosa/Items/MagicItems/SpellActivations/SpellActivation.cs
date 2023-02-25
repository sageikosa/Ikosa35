using Uzi.Core.Contracts;
using System;
using System.Linq;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// [Adjunct, IActionProvider]
    /// </summary>
    [Serializable]
    public abstract class SpellActivation : Adjunct, IActionProvider, IMagicAura, ICore, IAugmentationCost
    {
        #region Construction
        protected SpellActivation(SpellSource source, IPowerCapacity capacity, SpellActivationCost cost, bool affinity, string similarity)
            : base(source)
        {
            _Capacity = capacity;
            _Cost = cost;
            _Affinity = affinity;
            _Similarity = similarity;
        }
        #endregion

        #region private data
        private IPowerCapacity _Capacity;
        protected SpellActivationCost _Cost;
        private bool _Affinity;
        private string _Similarity;
        #endregion

        public SpellSource SpellSource => Source as SpellSource;
        public IPowerCapacity PowerCapacity => _Capacity;

        public abstract IEnumerable<CoreAction> GetActions(CoreActionBudget budget);

        public abstract Info GetProviderInfo(CoreActionBudget budget);

        public MagicStyle MagicStyle => SpellSource.MagicStyle;
        public int PowerLevel => SpellSource.PowerLevel;
        public int CasterLevel => SpellSource.CasterLevel;
        public AuraStrength MagicStrength => SpellSource.MagicStrength;
        public AuraStrength AuraStrength => MagicStrength;

        public abstract decimal LevelUnitPrice { get; }

        public decimal CastingMaterialPrice
            => (SpellSource.CasterClass.MagicType == MagicType.Divine)
            ? SpellSource.SpellDef.DivineComponents.OfType<CostlyMaterialComponent>().Sum(_cmc => _cmc.Cost)
            : SpellSource.SpellDef.ArcaneComponents.OfType<CostlyMaterialComponent>().Sum(_cmc => _cmc.Cost);

        public decimal MagicLevelPrice
            => (SpellSource.SlotLevel == 0)
            ? (LevelUnitPrice * 0.5m * CasterLevel) + CastingMaterialPrice
            : (LevelUnitPrice * CasterLevel * SpellSource.SlotLevel) + CastingMaterialPrice;

        public decimal StandardCost
            => _Cost.CalculatePrice(this);

        public bool Affinity => _Affinity;
        public string SimilarityKey => _Similarity;
    }
}
