using System;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa
{
    [Serializable]
    public class FeatParameter : IAdvancementOption, IResolveFeat
    {
        public FeatParameter(IResolveFeat target, Type type, string name, string description, int powerLevel)
        {
            Type = type;
            Name = name;
            Description = description;
            Target = target;
            PowerLevel = powerLevel;
        }
        public FeatParameter(Type type, string name, string description, int powerLevel)
        {
            Type = type;
            Name = name;
            Description = description;
            PowerLevel = powerLevel;
        }

        public Type Type { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public IResolveFeat Target { get; internal set; }
        public bool IsResolvable => Target != null;
        public int PowerLevel { get; private set; }

        public virtual FeatBase GetFeat(Type selectedType, object source)
            => Target?.GetFeat(Type, source);

        public virtual FeatBase ResolveFeat(AdvancementOptionInfo info, object source)
            => (info?.FullName == Type?.FullName)
            ? Target?.GetFeat(Type, source)
            : null;

        public virtual AdvancementOptionInfo ToAdvancementOptionInfo()
            => new AdvancementOptionInfo
            {
                FullName = Type?.FullName,
                Description = Description,
                Name = Name
            };

        public virtual IAdvancementOption GetOption(AdvancementOptionInfo optInfo)
            => (optInfo?.FullName.Equals(Type?.FullName, StringComparison.OrdinalIgnoreCase) ?? false)
            ? this
            : null;
    }
}
