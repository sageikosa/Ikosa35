using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa
{
    /// <summary>
    /// List a generic-parameterized feat (provides a mechanism to view type parameters, and set the specific feat)
    /// </summary>
    public class ParameterizedFeatListItem : FeatListItem, IResolveFeat
    {
        public ParameterizedFeatListItem(Type genericType, Creature creature, int powerLevel)
            : base(null)
        {
            GenericType = genericType;
            Info = FeatBase.ParameterizedFeatInfo(genericType);
            Creature = creature;
            PowerLevel = powerLevel;
            _Available = Info.GetAvailableTypes(this, Creature, PowerLevel).ToList();
        }

        #region data
        private List<FeatParameter> _Available;
        #endregion

        public ParameterizedFeatInfoAttribute Info { get; private set; }
        public Type GenericType { get; private set; }
        public Creature Creature { get; private set; }
        public int PowerLevel { get; private set; }

        public override string Name => Info.Name;
        public override string Benefit => Info.Benefit;
        public override string Description => Info.Benefit;

        /// <summary>
        /// Uses the ParameterizedFeatInfo attribute on the Feat class to get available types
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FeatParameter> AvailableParameters
            => _Available;
            //=> Info.GetAvailableTypes(this, Creature, PowerLevel);

        /// <summary>
        /// Returns a Feat of a specific type (and sets it to the Feat property as well)
        /// </summary>
        public FeatBase GetFeat(Type selectedType, object source)
        {
            Type _specific = GenericType.MakeGenericType(selectedType);
            Feat = (FeatBase)Activator.CreateInstance(_specific, source, PowerLevel);
            return Feat;
        }


        /// <summary>ResolveFeat if possible using AdvancementOptionInfo</summary>
        public override FeatBase ResolveFeat(AdvancementOptionInfo info, object source)
            => (info?.FullName == GenericType.FullName)
                ? (from _p in AvailableParameters
                   let _f = _p.ResolveFeat(info?.ParameterValue, source)
                   where _f != null
                   select _f).FirstOrDefault()
                : null;

        public override AdvancementOptionInfo ToAdvancementOptionInfo()
            => new AdvancementOptionInfo
            {
                FullName = GenericType.FullName,
                Description = Description,
                Name = Name,
                AvailableParameters = AvailableParameters.Select(_p => _p.ToAdvancementOptionInfo()).ToList()
            };

        public override IAdvancementOption GetOption(AdvancementOptionInfo optInfo)
            => (optInfo?.FullName.Equals(GenericType.FullName, StringComparison.OrdinalIgnoreCase) ?? false)
            ? AvailableParameters.FirstOrDefault(_p => _p.GetOption(optInfo.ParameterValue) != null)
            : null;
    }
}
