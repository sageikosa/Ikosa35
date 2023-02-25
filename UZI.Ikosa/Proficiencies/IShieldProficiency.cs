using Uzi.Ikosa.Items.Shields;

namespace Uzi.Ikosa
{
    public interface IShieldProficiency
    {
        bool IsProficientWithShield(bool tower, int powerLevel);
        bool IsProficientWith(ShieldBase shield, int powerLevel);
        string Description { get; }
    }
}
