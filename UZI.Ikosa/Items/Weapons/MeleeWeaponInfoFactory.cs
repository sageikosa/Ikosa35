using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Objects;
using Uzi.Core;

namespace Uzi.Ikosa.Items.Weapons
{
    public static class MeleeWeaponInfoFactory
    {
        public static MeleeWeaponInfo CreateMeleeWeaponInfo(CoreActor actor, WeaponBase weapon, bool baseValues)
        {
            var _melee = weapon as IMeleeWeapon;
            var _wpn = ObjectInfoFactory.CreateInfo<MeleeWeaponInfo>(actor, weapon, baseValues);
            _wpn.WeaponHeads = (from _head in _melee.AllHeads
                                select _head.ToWeaponHeadInfo(actor, baseValues)).ToArray();
            _wpn.ProficiencyType = weapon.ProficiencyType;
            _wpn.WieldTemplate = weapon.GetWieldTemplate();
            _wpn.IsFinessable = _melee.IsFinessable;
            _wpn.IsReachWeapon = _melee.IsReachWeapon;
            _wpn.AllowsOpportunisticAttacks = _melee.OpportunisticAttacks;
            if ((weapon is IThrowableWeapon)
                || (!baseValues && _melee.IsThrowable()))
            {
                _wpn.IsThrowable = true;
                var _throw = _melee.GetThrowable();
                if (_throw != null)
                {
                    _wpn.RangeIncrement = _throw.RangeIncrement;
                    _wpn.MaxRange = _throw.MaxRange;
                }
            }
            else
            {
                _wpn.IsThrowable = false;
            }
            return _wpn;
        }

        public static DoubleMeleeWeaponInfo CreateDoubleMeleeWeaponInfo(CoreActor actor, WeaponBase weapon, bool baseValues)
        {
            var _dbl = weapon as IMultiWeaponHead;
            var _wpn = ObjectInfoFactory.CreateInfo<DoubleMeleeWeaponInfo>(actor, weapon, baseValues);
            _wpn.WeaponHeads = (from _head in _dbl.AllHeads
                                select _head.ToWeaponHeadInfo(actor, baseValues)).ToArray();
            _wpn.ProficiencyType = weapon.ProficiencyType;
            _wpn.WieldTemplate = weapon.GetWieldTemplate();
            _wpn.IsFinessable = _dbl.IsFinessable;
            _wpn.IsReachWeapon = _dbl.IsReachWeapon;
            _wpn.AllowsOpportunisticAttacks = _dbl.OpportunisticAttacks;
            _wpn.UseAsTwoHanded = _dbl.UseAsTwoHanded;
            _wpn.IsDualWielding = _dbl.IsDualWielding;
            if ((weapon is IThrowableWeapon)
                || (!baseValues && _dbl.IsThrowable()))
            {
                _wpn.IsThrowable = true;
                var _throw = _dbl.GetThrowable();
                if (_throw != null)
                {
                    _wpn.RangeIncrement = _throw.RangeIncrement;
                    _wpn.MaxRange = _throw.MaxRange;
                }
            }
            else
            {
                _wpn.IsThrowable = false;
            }
            return _wpn;
        }
    }
}
