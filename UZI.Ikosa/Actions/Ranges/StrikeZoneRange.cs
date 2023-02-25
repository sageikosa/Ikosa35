using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class StrikeZoneRange : Range
    {
        public StrikeZoneRange(IMeleeWeapon weapon)
            : base()
        {
            _Weapon = weapon;
        }

        #region data
        private IMeleeWeapon _Weapon;
        #endregion

        public IMeleeWeapon Weapon => _Weapon;
        public StrikeCapture Zone
            => _Weapon?.Adjuncts.OfType<StrikeZoneMaster>().FirstOrDefault()?.ZoneLink.StrikeCapture;

        public override double EffectiveRange(CoreActor actor, int powerLevel)
        {
            // melee strike doesn't use effective range to select targets, it uses the strike capture zone
            return 0;
        }

        public override RangeInfo ToRangeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<StrikeZoneRangeInfo>(action, actor);

            var _critterReach = 1;
            var _critter = actor as Creature;
            if ((_critter != null) && (_critter.Body != null))
            {
                // creature natural reach (at least 0)
                _critterReach = _critter.Body.ReachSquares.EffectiveValue;
                if (_critterReach < 0)
                    _critterReach = 0;
            }
            var _maxReach = _critterReach;

            // target up close
            var _minReach = 0;
            if (Weapon.IsReachWeapon)
            {
                var _reach = Weapon as IReachWeapon;

                // reach weapons usually cannot target within natural reach (unless target adjacent)
                if (!_reach.TargetAdjacent)
                    _minReach = _critterReach + 1;

                // new max reach (reach weapon at least 1, otherwise add max reach again, then extra reach)
                if (_maxReach == 0)
                    _maxReach = 1; // NOTE: extra reach does not apply if natural reach is 0
                else
                    _maxReach += _maxReach + _reach.ExtraReach;
            }
            _info.MaximumReach = _maxReach;
            _info.MinimumReach = _minReach;
            return _info;
        }
    }
}
