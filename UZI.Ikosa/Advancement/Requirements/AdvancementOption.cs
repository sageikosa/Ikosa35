using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    [Serializable]
    public abstract class AdvancementOption : IAdvancementOption
    {
        protected AdvancementOption(IResolveRequirement target, string name, string description)
        {
            _Name = name;
            _Description = description;
            _Target = target;
        }

        #region data
        private string _Name;
        private string _Description;
        protected IResolveRequirement _Target;
        #endregion

        public virtual string Name => _Name;
        public virtual string Description => _Description;

        public virtual IResolveRequirement Target { get => _Target; internal set => _Target = value; }

        public virtual AdvancementOptionInfo ToAdvancementOptionInfo()
            => new AdvancementOptionInfo
            {
                Description = Description,
                Name = Name,
                FullName = $@"{typeof(AdvancementOption).FullName}.{Name}"
            };

        public abstract IAdvancementOption GetOption(AdvancementOptionInfo optInfo);
    }
}
