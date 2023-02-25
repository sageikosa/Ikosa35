using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.UI
{
    public class MeleeWeaponVM : PresentableThingVM<MeleeWeaponBase>
    {
        public List<MagicAugment> Augmentations
            => Thing.MainHead.Adjuncts
            .OfType<MagicAugment>()
            .Where(_ma => !(_ma.Augmentation is Enhanced))
            .ToList();

        public void DoAugmentationsChanged()
            => DoPropertyChanged(nameof(Augmentations));
    }
}