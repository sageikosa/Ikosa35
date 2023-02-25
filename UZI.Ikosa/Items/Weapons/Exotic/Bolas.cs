using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Items.Weapons
{
    [
        Serializable,
        WeaponHead(@"1d4", DamageType.Bludgeoning, 20, 2, typeof(Materials.WoodMaterial), Lethality.AlwaysNonLethal),
        ItemInfo(@"Bolas", @"Exotic One-Handed 1d4 (x2) Bludgeoning (Throw 10')", @"bolas")
    ]
    public class Bolas : MeleeWeaponBase, IExoticWeapon, IThrowableWeapon, IWieldMountable, ITrippingWeapon
    {
        public Bolas()
            : base(@"Bolas", WieldTemplate.OneHanded, false)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<Bolas>();
            ItemMaterial = Materials.WoodMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Exotic;
            Price.CorePrice = 5m;
            BaseWeight = 2d;
        }

        public override bool IsProficiencySuitable(Interaction interact)
            => (interact.InteractData is RangedAttackData);

        #region IThrowableWeapon Members
        public int RangeIncrement
            => CreaturePossessor?.Feats.Contains(typeof(Feats.FarShotFeat)) ?? false
            ? 20
            : 10;

        public virtual int MaxRange => RangeIncrement * 5;
        #endregion

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

        public override IEnumerable<AttackActionBase> WeaponStrikes()
        {
            yield return new ThrowStrike(MainHead, this.GetThrowable(), AttackImpact.Penetrating, @"101");
            // TODO: throw tripping strike...
            yield break;
        }

        protected override string ClassIconKey => @"bolas";

        public bool AvoidCounterByDrop => true;
    }
}
