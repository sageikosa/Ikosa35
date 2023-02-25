using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class ConveyanceObserveHandler : ObserveHandler
    {
        #region private IEnumerable<Cubic> OffsetCubes(Conveyance convey, IGeometricRegion rgn)
        private IEnumerable<Cubic> OffsetCubes(Conveyance convey, IGeometricRegion rgn)
        {
            var _loc = convey.GetLocated();
            if (_loc != null)
            {
                var _extents = convey.Orientation.CoverageExtents;
                var _cube = new Cubic(rgn.LowerZ, rgn.LowerY, rgn.LowerX, rgn.UpperZ, rgn.UpperY, rgn.UpperX);

                if (convey.Orientation.IsFaceSnapped(AnchorFace.ZLow, _extents))
                    yield return _cube.EdgeCubic(AnchorFace.ZLow).OffsetCubic(AnchorFace.ZLow);
                if (convey.Orientation.IsFaceSnapped(AnchorFace.ZHigh, _extents))
                    yield return _cube.EdgeCubic(AnchorFace.ZHigh).OffsetCubic(AnchorFace.ZHigh);
                if (convey.Orientation.IsFaceSnapped(AnchorFace.YLow, _extents))
                    yield return _cube.EdgeCubic(AnchorFace.YLow).OffsetCubic(AnchorFace.YLow);
                if (convey.Orientation.IsFaceSnapped(AnchorFace.YHigh, _extents))
                    yield return _cube.EdgeCubic(AnchorFace.YHigh).OffsetCubic(AnchorFace.YHigh);
                if (convey.Orientation.IsFaceSnapped(AnchorFace.XLow, _extents))
                    yield return _cube.EdgeCubic(AnchorFace.XLow).OffsetCubic(AnchorFace.XLow);
                if (convey.Orientation.IsFaceSnapped(AnchorFace.XHigh, _extents))
                    yield return _cube.EdgeCubic(AnchorFace.XHigh).OffsetCubic(AnchorFace.XHigh);
            }
            yield break;
        }
        #endregion

        #region protected override bool? ProcessAllLinesOfEffect(...)
        protected override bool? ProcessAllLinesOfEffect(Interaction workSet, Locator observerLocator,
            Locator targetLocator, List<SensoryBase> senses, ICoreObject target, ISensorHost sensors)
        {
            // default is normal processing
            // TODO: should probably re-visit with portal semantics in mind
            var _default = base.ProcessAllLinesOfEffect(workSet, observerLocator, targetLocator, senses, target, sensors);
            if (_default ?? false)
                return true;

            // try special furnishing code
            var _darkVisible = false;
            if (target is Conveyance _convey)
            {
                // get target region (and cubic box)
                var _rgn = targetLocator.GeometricRegion;
                foreach (var _cube in OffsetCubes(_convey, _rgn))
                {
                    // best range in the target
                    var _range = _cube.AllCellLocations().Max(_c => targetLocator.Map.GetLightLevel(_c));
                    var _maxDist = sensors?.Skills.Skill<SpotSkill>().MaxMultiDistance ?? 100;

                    var _factory = new SegmentSetFactory(observerLocator.Map, observerLocator.GeometricRegion, _cube,
                            ITacticalInquiryHelper.GetITacticals(sensors).ToArray(), SegmentSetProcess.Observation);

                    foreach (var _presence in GetDistinctPlanarPresences(senses))
                    {
                        if (_presence.HasOverlappingPresence(targetLocator.PlanarPresence))
                        {
                            // get all lines of effect (do not exclude the target, since we are trying to look past it (possibly))
                            foreach (var _lSet in observerLocator.LinesToTarget(_cube, _factory, _maxDist, _presence)
                            .Where(_l => _l.IsLineOfEffect))
                            {
                                // TODO: line of effect needs to be blocked by target!
                                var _check = CheckSenses(workSet, _lSet, senses, target, _range, _presence);
                                if (_check ?? false)
                                    return true;
                                if (_check == null)
                                    _darkVisible = true;
                            }
                        }
                    }
                }
            }

            // darkvisible, or whatever was originally processed
            return _darkVisible ? (bool?)null : _default;
        }
        #endregion
    }
}
