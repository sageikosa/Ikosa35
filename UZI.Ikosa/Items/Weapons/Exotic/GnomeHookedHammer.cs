using System;
using System.Collections.Generic;
using Uzi.Ikosa.Actions;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d8", DamageType.Bludgeoning, 20, 3, typeof(Materials.SteelMaterial), true, Contracts.Lethality.NormallyLethal),
    WeaponHead(@"1d6", DamageType.Piercing, 20, 4, typeof(Materials.SteelMaterial), false, Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Hooked Hammer, Gnomish", @"Exotic Double 1d8/1d6 (x3/x4) Bludgeoning/Piercing", @"hooked_hammer")
    ]
    public class GnomeHookedHammer: DoubleMeleeWeaponBase, IExoticWeapon, IWieldMountable, ITrippingWeapon
    {
        public GnomeHookedHammer()
            : base(@"Hammer, Gnome Hooked", false, Size.Small)
        {
            Setup();

            // default size is small (ie, a gnome)
            ItemSizer.ExpectedCreatureSize = Size.Small;
        }

        private void Setup()
        {
            _WeaponHeads.Add(GetWeaponHead<GnomeHookedHammer>(true));
            _WeaponHeads.Add(GetWeaponHead<GnomeHookedHammer>(false));
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Exotic;
            this.Price.CorePrice = 20m;
            this.BaseWeight = 6d;
            this.MaxStructurePoints.BaseValue = 10;
        }

        private IEnumerable<CoreAction> OriginalActions(CoreActionBudget budget)
        {
            return base.GetActions(budget);
        }

        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            var _fAtkBudget = FullAttackBudget.GetBudget(budget);

            // normal actions for a weapon
            foreach (CoreAction _act in OriginalActions(budget))
            {
                yield return _act;
            }
            yield break;
        }

        #region IWieldMountable Members

        public IEnumerable<string> SlotTypes
        {
            get
            {
                yield return ItemSlot.WieldMount;
                yield return ItemSlot.LargeWieldMount;
                yield break;
            }
        }

        #endregion

        protected override string ClassIconKey { get { return @"hooked_hammer"; } }

        #region ITrippingWeapon Members

        public bool AvoidCounterByDrop
        {
            get { return true; }
        }

        #endregion
    }
}
