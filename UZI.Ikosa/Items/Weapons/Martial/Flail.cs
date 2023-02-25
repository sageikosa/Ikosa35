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
    WeaponHead(@"1d8", DamageType.Bludgeoning, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Flail", @"Martial One-Handed 1d8 (x2) Bludgeoning", @"light_flail")
    ]
    public class Flail : MeleeWeaponBase, IMartialWeapon, IWieldMountable, ITrippingWeapon
    {
        // TODO: +2 disarm (conditional modifier attached to melee attack?)

        public Flail()
            : base(@"Flail", WieldTemplate.OneHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Flail>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 8m;
            this.BaseWeight = 5d;
            this.MaxStructurePoints.BaseValue = 5;
        }

        public IEnumerable<CoreAction> OriginalActions(CoreActionBudget budget)
        {
            return base.GetActions(budget);
        }

        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            LocalActionBudget _budget = budget as LocalActionBudget;
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

        protected override string ClassIconKey { get { return @"light_flail"; } }

        #region ITrippingWeapon Members

        public bool AvoidCounterByDrop
        {
            get { return true; }
        }

        #endregion
    }
}
