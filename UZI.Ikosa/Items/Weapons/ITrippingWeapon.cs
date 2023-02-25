namespace Uzi.Ikosa.Items.Weapons
{
    public interface ITrippingWeapon
    {
        bool AvoidCounterByDrop { get; }
    }

    public static class TrippingHelper
    {
        /// <summary>IThrowableWeapon or Throwing adjunct</summary>
        public static bool IsTrippingWeapon(this IMeleeWeapon self)
        {
            return (self is ITrippingWeapon);
        }
    }
}
