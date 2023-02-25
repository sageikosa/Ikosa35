using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    [Serializable]
    public class AdvancementParameter<ParamType> : AdvancementOption
    {
        public AdvancementParameter(string name, string description, ParamType paramValue) :
            base(null, name, description)
        {
            _ParamValue = paramValue;
        }

        public AdvancementParameter(IResolveRequirement target, string name, string description, ParamType paramValue) :
            base(target, name, description)
        {
            _ParamValue = paramValue;
        }

        private ParamType _ParamValue;

        public ParamType ParameterValue
            => _ParamValue;

        protected string _FullName
            => (_ParamValue as Type)?.FullName ?? _ParamValue?.GetType().FullName;

        public override AdvancementOptionInfo ToAdvancementOptionInfo()
            => new AdvancementOptionInfo
            {
                Name = Name,
                Description = Description,
                FullName = _FullName
            };

        public override IAdvancementOption GetOption(AdvancementOptionInfo optInfo)
            => (optInfo?.FullName.Equals(_FullName, StringComparison.OrdinalIgnoreCase) ?? false)
            ? this
            : null;
    }
}
