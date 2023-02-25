using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    [Serializable]
    public abstract class ParameterizedAdvancementOption : AdvancementOption, IParameterizedAdvancementOption,
        IResolveRequirement
    {
        public ParameterizedAdvancementOption(IResolveRequirement resolveTarget, string name, string description, 
            object parameterTarget, IEnumerable<IAdvancementOption> paramList)
            : base(resolveTarget, name, description)
        {
            _ParamList = paramList;
            ParameterTarget = parameterTarget;
        }

        private IEnumerable<IAdvancementOption> _ParamList;

        public IAdvancementOption ParameterValue { get; set; }
        public object ParameterTarget { get; private set; }

        protected virtual IAdvancementOption TransformOptionGet(IAdvancementOption advOption)
            => advOption;

        public IEnumerable<IAdvancementOption> AvailableParameters
            => _ParamList.Select(_opt => TransformOptionGet(_opt));

        /// <summary>
        /// override to yield the advancement option to push up the chain
        /// </summary>
        /// <param name="advOption"></param>
        /// <returns></returns>
        protected virtual IAdvancementOption TransformOptionSet(IAdvancementOption advOption)
            => advOption;

        public bool SetOption(IAdvancementOption advOption)
            => Target.SetOption(TransformOptionSet(advOption));
    }
}
