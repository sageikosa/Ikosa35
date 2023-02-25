using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    [
    Serializable,
    WeaponHead(@"1d10", DamageType.Slashing, 19, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Sword, Bastard", @"Exotic 1d10 (19-20/x2) Slashing (2-Handed Martial)", @"bastard_sword")
    ]
    public class BastardSword: MeleeWeaponBase, IExoticWeaponHeavy, IWieldMountable
    {
        public BastardSword(Size creatureSize)
            : base("Sword, Bastard", WieldTemplate.OneHanded, false)
        {
            Setup(creatureSize);
        }

        public BastardSword()
            : base("Sword, Bastard", WieldTemplate.OneHanded, false)
        {
            Setup(Size.Medium);
        }

        private void Setup(Size creatureSize)
        {
            _MainHead = GetWeaponHead<BastardSword>(true);
            this.ItemMaterial = Materials.SteelMaterial.Static;
            this.ProficiencyType = WeaponProficiencyType.Exotic;  // default
            this.Price.CorePrice = 35m;
            this.BaseWeight = 6d;
            this.MaxStructurePoints.BaseValue = 10; // special treatment (even though it is one-handed)
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

        protected override string ClassIconKey { get { return @"bastard_sword"; } }
    }
}
