using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Shields;

namespace Uzi.Ikosa.UI
{
    public class ShieldVM : PresentableThingVM<ShieldBase>
    {
        public List<MagicAugment> Augmentations
            => Thing.Adjuncts
            .OfType<MagicAugment>()
            .Where(_ma => !(_ma.Augmentation is Enhanced))
            .ToList();

        public void DoAugmentationsChanged()
            => DoPropertyChanged(nameof(Augmentations));
    }
}
