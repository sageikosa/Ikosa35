using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"2d4", new DamageType[] { DamageType.Piercing, DamageType.Slashing },
        20, 4, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Scythe", @"Martial Two-Handed 2d4 (x4) Piercing or Slashing", @"scythe")
    ]
    public class Scythe : MeleeWeaponBase, IMartialWeapon, ITrippingWeapon
    {
        public Scythe()
            : base(@"Scythe", WieldTemplate.TwoHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Scythe>();
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Martial;
            this.Price.CorePrice = 18m;
            this.BaseWeight = 10d;
            this.MaxStructurePoints.BaseValue = 10;
        }

        private IEnumerable<CoreAction> OriginalActions(CoreActionBudget budget)
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

        protected override string ClassIconKey { get { return @"scythe"; } }

        #region ITrippingWeapon Members

        public bool AvoidCounterByDrop
        {
            get { return true; }
        }

        #endregion
    }
}
