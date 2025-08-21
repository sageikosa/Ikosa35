using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Masterwork : Adjunct
    {
        /// <summary>Use typeof(Masterwork) for an independent masterwork adjunct</summary>
        public Masterwork(object source)
            : base(source)
        {
            _Delta = new Delta(1, typeof(Enhancement));
        }

        #region data
        private Delta _Delta;
        #endregion

        public Delta Delta => _Delta;

        public override bool CanAnchor(IAdjunctable newAnchor)
        {
            if (newAnchor != null)
            {
                // cannot already be masterwork
                if (this.IsOtherMasterwork(newAnchor))
                {
                    return false;
                }

                return base.CanAnchor(newAnchor);
            }

            // must be an item
            return false;
        }

        public override bool CanUnAnchor()
        {
            // prevent if independent and other independent adjuncts require enhancement...
            if (Anchor.IsEnhanced())
            {
                return false;
            }

            if (Anchor is IMeleeWeapon)
            {
                // melee weapon cannot become un-masterwork if a head is enhanced
                if ((Anchor as IMeleeWeapon).AllHeads.Any(_h => _h.IsEnhanced()))
                {
                    return false;
                }
            }
            return base.CanUnAnchor();
        }

        protected override void OnActivate(object source)
        {
            if (Anchor is IAmmunitionBase _ammo)
            {
                _ammo.AttackBonus.Deltas.Add(_Delta);
                _ammo.Price.CoreExtraPrice += 6;
            }
            else
            {
                if (Anchor is WeaponBase _wpn)
                {
                    // allow masterwork cost to be applied when activated (always on when anchored)
                    if (_wpn is IAttackSource)
                    {
                        // weapon itself is attack source
                        (_wpn as IAttackSource).AttackBonus.Deltas.Add(Delta);
                        _wpn.Price.CoreExtraPrice += 300m;
                    }
                    else if (_wpn is IMeleeWeapon)
                    {
                        // otherwise each weapon head is an attack source
                        var _melee = _wpn as IMeleeWeapon;
                        foreach (var _head in _melee.AllHeads)
                        {
                            _head.AttackBonus.Deltas.Add(Delta);
                        }
                        _wpn.Price.CoreExtraPrice += (300m * _melee.HeadCount);
                    }
                    else if (Anchor is IThrowableWeapon _throwable)
                    {
                        // throwable non-melee...
                        _throwable.MainHead.AttackBonus.Deltas.Add(Delta);
                        _wpn.Price.CoreExtraPrice += 300;
                    }
                }
                else
                {
                    if ((Anchor is IProtectorItem _protector) && (_protector.CheckPenalty.EffectiveValue < 0))
                    {
                        // allow masterwork cost to be applied when activated (always on when anchored)
                        // NOTE: not added if there is no check penalty
                        _protector.CheckPenalty.Deltas.Add(Delta);
                        _protector.Price.CoreExtraPrice += 150m;
                    }
                }
            }
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            // NOTE: this does not destroy the Delta, it can be reused without having to reconstruct
            Delta.DoTerminate();
            if (Anchor is IAmmunitionBase _ammo)
            {
                _ammo.Price.CoreExtraPrice -= 6;
            }
            else
            {
                if (Anchor is IMeleeWeapon _melee)
                {
                    _melee.Price.CoreExtraPrice -= (300m * _melee.HeadCount);
                }
                else if ((Anchor is IThrowableWeapon _throwable) && (Anchor is WeaponBase _wpn))
                {
                    // throwable non-melee...
                    _wpn.Price.CoreExtraPrice -= 300;
                }
                else
                {
                    if (Anchor is WeaponBase _wpnBase)
                    {
                        // remove masterwork cost when de-activated (always on when anchored)
                        _wpnBase.Price.CoreExtraPrice -= 300m;
                    }
                    else
                    {
                        if (Anchor is IProtectorItem _protector)
                        {
                            // remove masterwork cost when de-activated (always on when anchored)
                            _protector.Price.CoreExtraPrice -= 150m;
                        }
                    }
                }
            }

            base.OnDeactivate(source);
        }

        public override object Clone()
            => new Masterwork(Source);

        public override bool Equals(Adjunct other)
            => other is Masterwork;
    }

    public static class MasterworkHelper
    {
        /// <summary>True if an independent Masterwork adjunct is attached to object</summary>
        public static bool IsMasterwork(this IAdjunctable self)
        {
            return self.Adjuncts.Any(_a => _a is Masterwork);
        }

        public static bool IsOtherMasterwork(this Masterwork self, IAdjunctable target)
        {
            return target.Adjuncts.Any(_a => _a is Masterwork && _a != self);
        }
    }
}
