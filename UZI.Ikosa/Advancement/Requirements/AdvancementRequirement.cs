using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    [Serializable]
    public class AdvancementRequirement : IResolveRequirement
    {
        #region ctor()
        public AdvancementRequirement(RequirementKey key, string name, string description,
            Func<IResolveRequirement, RequirementKey, IEnumerable<IAdvancementOption>> options,
            Func<RequirementKey, IAdvancementOption, bool> setter,
            Func<RequirementKey, bool> checker)
        {
            Key = key;
            _Name = name;
            _Description = description;
            _AvailableOptions = options;
            _SetOption = setter;
            _Checker = checker;
            _Val = null;
        }
        #endregion

        #region data
        private string _Name;
        private string _Description;
        private IFeature _Val;
        private Func<IResolveRequirement, RequirementKey, IEnumerable<IAdvancementOption>> _AvailableOptions;
        private Func<RequirementKey, IAdvancementOption, bool> _SetOption;
        private Func<RequirementKey, bool> _Checker;
        #endregion

        public RequirementKey Key { get; private set; }
        public virtual string Name => _Name;
        public virtual string Description => _Description;
        public IFeature CurrentValue { get => _Val; set => _Val = value; }

        public virtual IEnumerable<IAdvancementOption> AvailableOptions
            => _AvailableOptions(this, Key).Select(_opt => _opt).ToList();

        public virtual bool SetOption(IAdvancementOption advOption)
            => _SetOption?.Invoke(Key, advOption) ?? false;

        public virtual bool IsSet => _Checker?.Invoke(Key) ?? false;

        public AdvancementRequirementInfo ToAdvancementRequirementInfo()
            => new AdvancementRequirementInfo
            {
                CurrentValue = CurrentValue?.ToFeatureInfo(),
                Description = Description,
                Name = Name,
                Key = Key.ToRequirementKeyInfo(),                 
                AvailableOptions = AvailableOptions.Select(_opt => _opt.ToAdvancementOptionInfo()).ToList()
            };

        public bool SetAdvancementOption(AdvancementOptionInfo optInfo)
        {
            var _opt = AvailableOptions.FirstOrDefault(_o => _o.GetOption(optInfo) != null);
            if (_opt != null)
            {
                return _SetOption?.Invoke(Key, _opt) ?? false;
            }
            return false;
        }
    }
}
