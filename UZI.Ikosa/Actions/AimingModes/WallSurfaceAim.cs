using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Used to attach to wall surfaces
    /// </summary>
    [Serializable]
    public class WallSurfaceAim : RangedAim
    {
        public WallSurfaceAim(string key, string displayName, Range minModes, Range maxModes, Range range,
            Range maxLength, Range maxHeight, Range minFaceSquareSide)
            : base(key, displayName, minModes, maxModes, range)
        {
            MaxLength = maxLength;
            MaxHeight = maxHeight;
            MinFaceSquareSide = minFaceSquareSide;
        }

        /// <summary>Maximum length in squares</summary>
        public Range MaxLength { get; private set; }
        /// <summary>Maximum height in squares</summary>
        public Range MaxHeight { get; private set; }
        /// <summary>Minimum selectable size of square in cell units</summary>
        public Range MinFaceSquareSide { get; private set; }

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        {
            // must be within max range
            var _max = Range.EffectiveRange(actor, action.CoreActionClassLevel(actor, this));

            // TODO: check against parameters?
            var _loc = actor.GetLocated().Locator;
            return from _wsti in SelectedTargets<WallSurfaceTargetInfo>(actor, action, infos)
                   where _loc.GeometricRegion.NearDistanceToCell(_wsti.CellInfo) <= _max
                   select new WallSurfaceTarget(Key, new Intersection(_wsti.CellInfo), (AnchorFace)_wsti.AnchorFace, _loc.MapContext);
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToRangedAimInfo<WallSurfaceAimInfo>(action, actor);
            _info.MaxLength = MaxLength.ToRangeInfo(action, actor);
            _info.MaxHeight = MaxHeight.ToRangeInfo(action, actor);
            _info.MinFaceSquareSide = MinFaceSquareSide.ToRangeInfo(action, actor);
            return _info;
        }
    }
}
