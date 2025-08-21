using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Items.Weapons
{
    public interface IThrowableWeapon : IRangedSource
    {
        IWeaponHead MainHead { get; }
    }

    public static class ThrowableHelper
    {
        /// <summary>IThrowableWeapon or Throwing adjunct</summary>
        public static bool IsThrowable(this IMeleeWeapon self)
        {
            return (self is IThrowableWeapon) || self.HasActiveAdjunct<Throwing>();
        }

        public static IThrowableWeapon GetThrowable(this IMeleeWeapon self)
        {
            if (self is IThrowableWeapon)
            {
                return self as IThrowableWeapon;
            }

            return self.Adjuncts.OfType<Throwing>().FirstOrDefault(_t => _t.IsActive);
        }
    }
}
