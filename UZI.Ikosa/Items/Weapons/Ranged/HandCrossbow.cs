using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    // NOTE: hand crossbow bolts should probably be different than regular crossbow bolts
    [
    Serializable,
    WeaponHead(@"1d4", DamageType.Piercing, 19, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Crossbow, Hand", @"Exotic Ranged 1d4 (19-20/x2) Piercing", @"hand_xbow")
    ]
    public class HandCrossbow : CrossbowBase, IExoticWeapon, IWieldMountable
    {
        public HandCrossbow()
            : base(@"Crossbow, Hand", 30, Size.Tiny)
        {
            Setup();
        }

        private void Setup()
        {
            ItemMaterial = Materials.WoodMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Exotic;
            Price.CorePrice = 100m;
            MaxStructurePoints.BaseValue = 2;
            BaseWeight = 2d;
        }

        protected override IWeaponHead GetProxyHead() { return GetWeaponHead<HandCrossbow>(true); }

        public override TimeType ReloadAction
            => (CreaturePossessor?.Feats.Contains(typeof(Feats.RapidReloadFeat<HandCrossbow>)) ?? false)
            ? TimeType.Free
            : TimeType.Brief;

        #region IWieldMountable Members

        public IEnumerable<string> SlotTypes
        {
            get
            {
                yield return ItemSlot.WieldMount;
                yield break;
            }
        }

        #endregion

        protected override string ClassIconKey => @"hand_xbow";
    }
}
