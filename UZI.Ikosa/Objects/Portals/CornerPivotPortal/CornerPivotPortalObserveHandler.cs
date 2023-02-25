using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Senses;
using Uzi.Visualize;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Movement;
using System.Diagnostics;
using System.Text;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class CornerPivotPortalObserveHandler : ObserveHandler
    {
        private IEnumerable<Cubic> OffsetCubes(CornerPivotPortal portal, IGeometricRegion rgn)
        {
            var _cube = new Cubic(rgn.LowerZ, rgn.LowerY, rgn.LowerX, rgn.UpperZ, rgn.UpperY, rgn.UpperX);
            if (portal.OpenState.Value < 1)
                yield return _cube
                    .EdgeCubic(portal.AnchorClose)
                    .OffsetCubic(portal.AnchorClose);
            if (!portal.OpenState.IsClosed)
                yield return _cube
                    .EdgeCubic(portal.AnchorOpen)
                    .OffsetCubic(portal.AnchorOpen);
            yield break;
        }

        #region protected override bool? ProcessAllLinesOfEffect(...)
        protected override bool? ProcessAllLinesOfEffect(Interaction workSet, Locator observerLocator,
            Locator targetLocator, List<SensoryBase> senses, ICoreObject target, ISensorHost sensors)
        {
            // default is normal processing
            bool? _default()
                => base.ProcessAllLinesOfEffect(workSet, observerLocator, targetLocator, senses, target, sensors);

            // offset cubic processing
            bool? _offset(Cubic _cube)
            {
                var _darkVisible = false;

                // best range in the target
                var _range = _cube.AllCellLocations().Max(_c => targetLocator.Map.GetLightLevel(_c));
                var _maxDist = sensors?.Skills.Skill<SpotSkill>().MaxMultiDistance ?? 100;

                var _factory = new SegmentSetFactory(observerLocator.Map, observerLocator.GeometricRegion, _cube,
                        ITacticalInquiryHelper.GetITacticals(sensors).ToArray(), SegmentSetProcess.Observation);

                foreach (var _presence in GetDistinctPlanarPresences(senses))
                {
                    if (_presence.HasOverlappingPresence(targetLocator.PlanarPresence))
                    {
                        // get all lines of effect (do not exclude the portal, since we are trying to look past it (possibly))
                        foreach (var _lSet in observerLocator.LinesToTarget(_cube, _factory, _maxDist, _presence)
                        .Where(_l => _l.IsLineOfEffect))
                        {
                            // TODO: line of effect needs to be blocked by portal!
                            var _check = CheckSenses(workSet, _lSet, senses, target, _range, _presence);
                            if (_check ?? false)
                            {
                                // real true
                                return true;
                            }

                            // null means dark visible
                            if (_check == null)
                                _darkVisible = true;
                        }
                    }
                }

                // finished with dark visible carries through
                return _darkVisible ? (bool?)null : false;
            };

            // otherwise, try special corner pivot portal code
            if (target is CornerPivotPortal _portal)
            {
                // get target region (and cubic box)
                var _rgn = targetLocator.GeometricRegion;

                var _altCube = OffsetCubes(_portal, _rgn).FirstOrDefault();
                if (_altCube != null)
                {
                    // open or closed, if alt cube is closer
                    if (observerLocator.GeometricRegion.NearDistance(_rgn) > observerLocator.GeometricRegion.NearDistance(_altCube))
                        return _offset(_altCube);
                }
            }
            return _default();
        }
        #endregion
    }
}