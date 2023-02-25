using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa
{
    /// <summary>
    /// Feat parameter that allows deeper chaining and sub-parameters
    /// </summary>
    [Serializable]
    public class ParameterizedFeatParameter : FeatParameter
    {
        public ParameterizedFeatParameter(ParameterizedFeatListItem target, Type type, string name, string description,
            int powerLevel, List<FeatParameter> subParameters)
            : base(target, type, name, description, powerLevel)
        {
            _SubParameters = subParameters;
        }

        private List<FeatParameter> _SubParameters;

        public override FeatBase GetFeat(Type selectedType, object source)
        {
            Type _concrete = Type.MakeGenericType(selectedType);
            return Target.GetFeat(_concrete, source);
        }

        public override FeatBase ResolveFeat(AdvancementOptionInfo info, object source)
            => (info?.FullName == Type?.FullName)
                ? (from _p in _SubParameters
                   let _f = _p.ResolveFeat(info?.ParameterValue, source)
                   where _f != null
                   select _f).FirstOrDefault()
                : null;

        public override AdvancementOptionInfo ToAdvancementOptionInfo()
            => new AdvancementOptionInfo
            {
                FullName = Type?.FullName,
                Description = Description,
                Name = Name,
                AvailableParameters = _SubParameters.Select(_p => _p.ToAdvancementOptionInfo()).ToList()
            };

        public override IAdvancementOption GetOption(AdvancementOptionInfo optInfo)
            => (optInfo?.FullName.Equals(Type?.FullName, StringComparison.OrdinalIgnoreCase) ?? false)
            ? _SubParameters.FirstOrDefault(_p => _p.GetOption(optInfo.ParameterValue) != null)
            : null;
    }
}
