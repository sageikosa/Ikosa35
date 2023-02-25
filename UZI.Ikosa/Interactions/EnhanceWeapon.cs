using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    // TODO: enhance weapon
    public class EnhanceWeapon : InteractData
    {
        public EnhanceWeapon(CoreActor actor)
            : base(actor)
        {
        }

        public int NewBonus { get; private set; }
    }
}
