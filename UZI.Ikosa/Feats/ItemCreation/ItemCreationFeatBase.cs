using System;

namespace Uzi.Ikosa.Feats
{
    [Serializable]
    public abstract class ItemCreationFeatBase: FeatBase
    {
        protected ItemCreationFeatBase(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }
        // TODO: common item creation interaction
    }
}
