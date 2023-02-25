using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d10", DamageType.Slashing, 20, 3, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Waraxe, Dwarven", @"Exotic 1d10 (x3) Slashing (2-Handed Martial)", @"waraxe")
    ]
    public class DwarvenWaraxe : MeleeWeaponBase, IExoticWeaponHeavy, IWieldMountable
    {
        public DwarvenWaraxe()
            : base("Waraxe, Dwarven", WieldTemplate.OneHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<DwarvenWaraxe>(true);
            this.ItemMaterial = Materials.WoodMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Exotic;  // default
            this.Price.CorePrice = 30m;
            this.BaseWeight = 8d;
            this.MaxStructurePoints.BaseValue = 10; // although this is a one-handed weapon, without proficiency, it is two-handed
        }

        /// <summary>ensures the user can get actions from the weapon via the way it is wielded</summary>
        protected override void OnSetItemSlot()
        {
            // must be proficient, or using two hands
            if (CreaturePossessor.Proficiencies.IsProficientWith(this, CreaturePossessor.AdvancementLog.NumberPowerDice)
                || (SecondarySlot != null))
            {
                // all the stuff for a weapon
                base.OnSetItemSlot();
            }
            else
            {
                //... just notification and adjunct
                base.DoSlotSet();
            }
        }

        public override WeaponProficiencyType ProficiencyType
        {
            get
            {
                // if wielding two-handed, changes to a martial weapon
                if ((this.MainSlot != null) && (this.SecondarySlot != null))
                {
                    return WeaponProficiencyType.Martial;
                }
                return base.ProficiencyType;
            }
        }

        #region IWieldMountable Members

        public IEnumerable<string> SlotTypes
        {
            get
            {
                yield return ItemSlot.LargeWieldMount;
                yield break;
            }
        }

        #endregion

        protected override string ClassIconKey { get { return @"waraxe"; } }
    }
}
