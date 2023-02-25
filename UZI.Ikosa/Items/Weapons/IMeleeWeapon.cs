using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Items.Weapons
{
    public interface IMeleeWeapon : IWeapon, IActionProvider
    {
        /// <summary>Can use dexterity for attack roll</summary>
        bool IsFinessable { get; }

        IWeaponHead MainHead { get; }
        int HeadCount { get; }
        IEnumerable<IWeaponHead> AllHeads { get; }

        /// <summary>True if weapon can make opportunistic attacks</summary>
        bool OpportunisticAttacks { get; }

        /// <summary>If weapon doesn't currently threaten, this may be null</summary>
        Geometry GetStrikeZone(bool snapshot = false);

        bool IsReachWeapon { get; }

        /// <summary>True if weapon has a penalty using a lethal choice option other than its default</summary>
        bool HasPenaltyUsingLethalChoice { get; }
    }

    public interface IMultiWeaponHead : IMeleeWeapon
    {
        IWeaponHead this[int index] { get; }
        IWeaponHead OffHandHead { get; }
        bool UseAsTwoHanded { get; }
        bool IsDualWielding { get; }
    }

    public static class MeleeWeaponHelper
    {
        /// <summary>Builds threat zone adjunct group and member with GetThreatZone geometry (if not already attached)</summary>
        public static void CreateStrikeZone(this IMeleeWeapon self)
        {
            // ensure cleanup
            self.RemoveStrikeZone();

            // create new
            // ??? var _loc = self.GetLocated();
            if (!self.HasAdjunct<StrikeZoneMaster>())
            {
                // threat zone setup
                var _link = new StrikeZoneLink();
                var _member = new StrikeZoneMaster(self, _link);
                self.AddAdjunct(_member);
            }
        }

        /// <summary>Removes threat zone adjunct group and member (if defined)</summary>
        public static void RemoveStrikeZone(this IMeleeWeapon self)
        {
            // threat zone removal
            self.Adjuncts.OfType<StrikeZoneMaster>().FirstOrDefault()?.Eject();
        }

        #region public static Geometry CreateStrikeGeometry(this IMeleeWeapon self)
        /// <summary>Defines the strike zone geometry</summary>
        public static Geometry CreateStrikeGeometry(this IMeleeWeapon self, bool snapshot)
        {
            var _critter = self.Possessor as Creature;
            var _loc = Locator.FindFirstLocator(_critter);
            if (_loc == null)
                return null;

            var _expectEven = _loc.LocationAimMode == LocationAimMode.Intersection;

            // creature natural reach (at least 0)
            var _critterReach = _critter.Body.ReachSquares.EffectiveValue;
            if (_critterReach < 0)
                _critterReach = 0;
            var _maxReach = _critterReach;

            // natural reach geometry
            var _critterSize = (_loc.Chief == _critter
                ? _critter.Body.Sizer.Size.CubeSize()
                : _loc.NormalSize as IGeometricSize);
            var _grabSize = new GeometricSize(
                _critterSize.ZHeight + (_critterReach * 2),
                _critterSize.YLength + (_critterReach * 2),
                _critterSize.XLength + (_critterReach * 2));
            var _grabZone = new CubicBuilder(_grabSize, _grabSize.CenterCell(_expectEven));

            // reach weapon (must also be sized for the creature's size or larger)
            if ((self is IReachWeapon _reach) && (self.ItemSizer.EffectiveCreatureSize.Order >= _critter.Body.Sizer.Size.Order))
            {
                // new max reach (reach weapon at least 1, otherwise add max reach again, then extra reach)
                if (_maxReach == 0)
                    _maxReach = 1; // NOTE: extra reach does not apply if natural reach is 0
                else
                    _maxReach += _maxReach + _reach.ExtraReach;

                // weapon reach geometry (critter size + maxreach twice over)
                var _reachSize = new GeometricSize(
                    _critterSize.ZHeight + (_maxReach * 2),
                    _critterSize.YLength + (_maxReach * 2),
                    _critterSize.XLength + (_maxReach * 2));
                var _reachZone = new CubicBuilder(_reachSize, _reachSize.CenterCell(_expectEven));
                if (_reach.TargetAdjacent)
                {
                    return new Geometry(_reachZone, _loc as IGeometryAnchorSupplier, snapshot);
                }
                else
                {
                    var _multi = new MultiBuilder(new IGeometryBuilder[] { _reachZone }, new IGeometryBuilder[] { _grabZone });
                    return new Geometry(_multi, _loc as IGeometryAnchorSupplier, snapshot);
                }
            }
            else
            {
                return new Geometry(_grabZone, _loc as IGeometryAnchorSupplier, snapshot);
            }
        }
        #endregion
    }
}
