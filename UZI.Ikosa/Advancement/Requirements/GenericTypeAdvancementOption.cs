using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    /// <summary>
    /// Resolves advancement parameters to specific instances of generic types
    /// </summary>
    [Serializable]
    public class GenericTypeAdvancementOption : ParameterizedAdvancementOption
    {
        /// <summary>
        /// Resolves advancement parameters to specific instances of generic types
        /// </summary>
        public GenericTypeAdvancementOption(IResolveRequirement resolveTarget, string name, string description,
            object parameterTarget, IEnumerable<IAdvancementOption> paramList)
            : base(resolveTarget, name, description, parameterTarget, paramList)
        {
        }

        protected override IAdvancementOption TransformOptionGet(IAdvancementOption advOption)
        {
            var _option = advOption as AdvancementParameter<Type>;
            if (_option != null)
            {
                _option.Target = this;
            }
            return _option;
        }

        protected override IAdvancementOption TransformOptionSet(IAdvancementOption advOption)
        {
            if (advOption is AdvancementParameter<Type> _option)
            {
                return new AdvancementParameter<Type>(this, advOption.Name, advOption.Description,
                    ((Type)ParameterTarget).MakeGenericType(_option.ParameterValue));
            }
            return advOption;
        }

        public override AdvancementOptionInfo ToAdvancementOptionInfo()
            => new AdvancementOptionInfo
            {
                FullName = $@"{GetType().FullName}:{Name}",
                Name = Name,
                Description = Description,
                ParameterValue = ParameterValue?.ToAdvancementOptionInfo(),
                AvailableParameters = AvailableParameters.Select(_p => _p.ToAdvancementOptionInfo()).ToList()
            };

        public override IAdvancementOption GetOption(AdvancementOptionInfo optInfo)
            => (optInfo?.FullName.Equals($@"{GetType().FullName}:{Name}", StringComparison.OrdinalIgnoreCase) ?? false)
            ? AvailableParameters.FirstOrDefault(_p => _p.GetOption(optInfo.ParameterValue) != null)
            : null;
    }
}
