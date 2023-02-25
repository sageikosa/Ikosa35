using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Shapeable volumetric aim.  Expected to use GeometricRegionTarget.</summary>
    [Serializable]
    public class VolumeAim : RangedAim
    {
        /// <summary>Shapeable volumetric aim.  Expected to use GeometricRegionTarget.</summary>
        public VolumeAim(string key, string displayName, Range minModes, Range maxModes, Range range, Range cubeSize)
            : base(key, displayName, minModes, maxModes, range)
        {
            _CubeSize = cubeSize;
        }

        private Range _CubeSize;
        public Range CubeSize => _CubeSize;

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        {
            // must be within max range
            var _max = Range.EffectiveRange(actor, action.CoreActionClassLevel(actor, this));

            // validate cube sizes
            var _maxCube = CubeSize.EffectiveRange(actor, action.CoreActionClassLevel(actor, this));

            // return cubic targets
            var _loc = actor.GetLocated().Locator;
            return from _cti in SelectedTargets<CubicTargetInfo>(actor, action, infos)
                   // cubic must be within size bounds
                   where _cti.Cubic.XLength <= _maxCube
                   && _cti.Cubic.YLength <= _maxCube
                   && _cti.Cubic.ZHeight <= _maxCube
                   // cell list of cells in range
                   let _region = new CellList(
                       _cti.Cubic.AllCellLocations().Where(_cl => _loc.GeometricRegion.NearDistanceToCell(_cl) <= _max),
                       _maxCube, _maxCube, _maxCube)
                   where _region.HasAnyCells
                   select new GeometricRegionTarget(Key, _region, _loc.MapContext);
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToRangedAimInfo<VolumeAimInfo>(action, actor);
            _info.CubeSize = CubeSize.ToRangeInfo(action, actor);
            return _info;
        }
    }
}
