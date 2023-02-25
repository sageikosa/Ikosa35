using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{

    [Serializable]
    public class SlidingPortalObserveHandler : ObserveHandler
    {
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

            if (target is SlidingPortal _portal)
            {
                // get target region (and cubic box)
                var _rgn = targetLocator.GeometricRegion;
                var _cube = new Cubic(_rgn.LowerZ, _rgn.LowerY, _rgn.LowerX, _rgn.UpperZ, _rgn.UpperY, _rgn.UpperX);

                // get offset cube based on portal anchor
                if (_portal.OpenState.Value < 1)
                {
                    _cube = _cube.EdgeCubic(_portal.AnchorFace).OffsetCubic(_portal.AnchorFace);

                    // open or closed, if cube is closer
                    if (observerLocator.GeometricRegion.NearDistance(_rgn) > observerLocator.GeometricRegion.NearDistance(_cube))
                        return _offset(_cube);
                }
            }
            return _default();
        }
    }
}
