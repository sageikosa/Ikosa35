using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Aimed at a point in space.  Can be used to indicate direction, 
    /// and to mark end-points of a distant line.
    /// </summary>
    [Serializable]
    public class LocationAim : RangedAim
    {
        /// <summary>
        /// Aimed at a point in space.  Can be used to indicate direction, 
        /// and to mark end-points of a distant line.
        /// </summary>
        public LocationAim(string key, string displayName, LocationAimMode mode, Range minModes, Range maxModes, Range range)
            : base(key, displayName, minModes, maxModes, range)
        {
            LocationAimMode = mode;
        }

        public LocationAimMode LocationAimMode { get; set; }

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        {
            var _aLoc = actor.GetLocated().Locator;
            var _powerLevel = action.CoreActionClassLevel(actor, this);

            // cell units
            var _range = Range as ICellRange;
            if (_range != null)
            {
                var _source = _range.GetTargetVolume(actor, _powerLevel, _aLoc.GeometricRegion);
                return from _it in SelectedTargets<LocationTargetInfo>(actor, action, infos)
                       where _source.ContainsCell(_it.CellInfo)
                       && ((LocationAimMode & _it.LocationAimMode) == LocationAimMode.Cell)
                       select new LocationTarget(Key, _it.LocationAimMode, _it.CellInfo, _aLoc.MapContext);
            }

            // scale units
            var _max = Range.EffectiveRange(actor, _powerLevel);
            return from _it in SelectedTargets<LocationTargetInfo>(actor, action, infos)
                   where _aLoc.GeometricRegion.NearDistance(_it.GetPoint3D()) <= _max
                   && ((LocationAimMode & _it.LocationAimMode) > LocationAimMode.None)
                   && ((LocationAimMode & _it.LocationAimMode) < LocationAimMode.Any)
                   select new LocationTarget(Key, _it.LocationAimMode, _it.CellInfo, _aLoc.MapContext);
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToRangedAimInfo<LocationAimInfo>(action, actor);
            _info.LocationAimMode = LocationAimMode;
            return _info;
        }
    }
}
