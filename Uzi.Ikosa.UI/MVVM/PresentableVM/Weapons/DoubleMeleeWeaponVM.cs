using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.UI
{
    public class DoubleMeleeWeaponVM : PresentableThingVM<DoubleMeleeWeaponBase>
    {
        public List<MagicAugment> MainAugmentations
            => Thing.MainHead.Adjuncts
            .OfType<MagicAugment>()
            .Where(_ma => !(_ma.Augmentation is Enhanced))
            .ToList();

        public List<MagicAugment> OffHandAugmentations
            => Thing.OffHandHead.Adjuncts
            .OfType<MagicAugment>()
            .Where(_ma => !(_ma.Augmentation is Enhanced))
            .ToList();

        public void DoAugmentationsChanged()
        {
            DoPropertyChanged(nameof(MainAugmentations));
            DoPropertyChanged(nameof(OffHandAugmentations));
        }
    }
}
