using System;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa
{
    /// <summary>
    /// List a non-parameterized feat
    /// </summary>
    public class FeatListItem : IAdvancementOption
    {
        public FeatListItem(FeatBase feat)
        {
            Feat = feat;
        }

        public FeatBase Feat { get; protected set; }

        public virtual string Name => Feat.Name;
        public virtual string Benefit => Feat.Benefit;
        public virtual string Description => Feat.Benefit;

        /// <summary>ResolveFeat if possible using AdvancementOptionInfo</summary>
        public virtual FeatBase ResolveFeat(AdvancementOptionInfo info, object source)
            => (info?.FullName == Feat.GetType().FullName)
                ? Feat
                : null;

        public virtual AdvancementOptionInfo ToAdvancementOptionInfo()
            => new AdvancementOptionInfo
            {
                FullName = Feat.GetType().FullName,
                Name = Name,
                Description = Benefit
            };

        public virtual IAdvancementOption GetOption(AdvancementOptionInfo optInfo)
            => (optInfo?.FullName.Equals(Feat.GetType().FullName, StringComparison.OrdinalIgnoreCase) ?? false)
            ? this
            : null;
    }
}
