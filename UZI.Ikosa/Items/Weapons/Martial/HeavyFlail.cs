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
    WeaponHead(@"1d10", DamageType.Bludgeoning, 19, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Flail, Heavy", @"Simple Light 1d10 (19-20/x2) Bludgeoning", @"heavy_flail")
    ]
    public class HeavyFlail : MeleeWeaponBase, IMartialWeapon, IWieldMountable, ITrippingWeapon
    {
        // TODO: +2 disarm

        public HeavyFlail()
            : base("Flail, Heavy", WieldTemplate.TwoHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<HeavyFlail>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 15m;
            this.BaseWeight = 10d;
            this.MaxStructurePoints.BaseValue = 10;
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

        protected override string ClassIconKey { get { return @"heavy_flail"; } }

        #region ITrippingWeapon Members

        public bool AvoidCounterByDrop
        {
            get { return true; }
        }

        #endregion
    }
}
