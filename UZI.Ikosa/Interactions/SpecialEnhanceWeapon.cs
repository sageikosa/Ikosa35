using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Interactions
{
    // TODO: special enhance weapon
    public class SpecialEnhanceWeapon : InteractData
    {
        public SpecialEnhanceWeapon(CoreActor actor)
            : base(actor)
        {
        }
        public WeaponSpecialAbility SpecialAbility { get; private set; }
    }
}
