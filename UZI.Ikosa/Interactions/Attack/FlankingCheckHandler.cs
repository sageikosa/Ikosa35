using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class FlankingCheckHandler : IInteractHandler
    {
        // If line between any center of ally cells transits opposite borders of target, then target flanked
        // Each ally must have target in a melee strike zone
        // Allies with no reach can’t participate in flank

        // improved uncanny dodge: alteration filter?

        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet?.InteractData is MeleeAttackData) || (workSet?.InteractData is ReachAttackData))
            {
                var _atk = workSet.InteractData as AttackData;
                if ((_atk.Attacker is Creature _attacker) && (workSet.Target is Creature _target))
                {
                    // line from any attacker cell center must go through one pair of Hi/Lo faces of target
                    // ... to cell center of a "friendly" creature that is threatening and unallied with target
                    var _atkLoc = _attacker.GetLocated()?.Locator;
                    var _atkGeom = _atkLoc?.GeometricRegion;
                    var _map = _atkLoc?.Map;

                    var _tgtLoc = _target.GetLocated()?.Locator;
                    var _tgtGeom = _tgtLoc?.GeometricRegion;
                    if ((_map != null) && (_atkGeom != null) && (_tgtGeom != null))
                    {
                        var _atkPresence = _atkLoc.PlanarPresence;
                        var _tgtPresence = _tgtLoc.PlanarPresence;

                        if (_atkPresence.HasOverlappingPresence(_tgtPresence))
                        {
                            // find friendly creatures in initiative that are threatening
                            var _threats = OpportunisticInquiry.GetThreateningCreatures(_target)
                                .Where(_c => _c.ID == _attacker.ID  // ignore the attacker
                                && _c.IsFriendly(_attacker.ID)      // must be friendly to attacker
                                && !_c.IsFriendly(_target.ID))      // must not be friendly to target
                                .ToList();

                            // cycle through all threats, even if we find one (in case)
                            var _factory = new SegmentSetFactory(_map, _atkGeom, _tgtGeom,
                                ITacticalInquiryHelper.EmptyArray, SegmentSetProcess.Geometry);
                            foreach (var _threat in from _ally in _threats
                                                    let _l = _ally.GetLocated()?.Locator
                                                    let _g = _l?.GeometricRegion
                                                    where _g != null
                                                    && _l.PlanarPresence.HasOverlappingPresence(_tgtPresence)   // planar
                                                    select new { Geometry = _g, Presense = _l.PlanarPresence, Flanker = _ally })
                            {
                                var _allyChecked = false;
                                foreach (var _aCell in _atkGeom.AllCellLocations())
                                {
                                    var _aPt = _aCell.GetPoint();
                                    foreach (var _gCell in _threat.Geometry.AllCellLocations())
                                    {
                                        // trace lines between centers of cells
                                        var _lSet = _map.SegmentCells(_aPt, _gCell.GetPoint(), _factory, PlanarPresence.None);

                                        // first and last cells in the line that contain the target geometry
                                        var _first = _lSet.AllIntermediate().FirstOrDefault(_lcr => _tgtGeom.ContainsCell(_lcr));
                                        if (_first.IsActual)
                                        {
                                            var _last = _lSet.AllIntermediate().LastOrDefault(_lcr => _tgtGeom.ContainsCell(_lcr));
                                            if (_last.IsActual)
                                            {
                                                // must be on opposite sides
                                                if (_first.EntryFaces == _last.ExitFaces.Invert())
                                                {
                                                    // flanking achieved for this "ally"
                                                    workSet.InteractData.Alterations.Add(_target, new FlankingAlteration(_atk, 2, _threat.Flanker));
                                                    if (workSet.InteractData.Alterations.Contains(typeof(FlankingAlteration)))
                                                        return;

                                                    // checked
                                                    _allyChecked = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    // since this ally flanked, skip extra cell checks
                                    if (_allyChecked)
                                        break;
                                }
                            }
                        }
                    }
                }

            }
        }

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(MeleeAttackData);
            yield return typeof(ReachAttackData);
            yield return typeof(RangedAttackData);
            yield break;
        }
        #endregion

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => (existingHandler.GetType() != typeof(TransitAttackHandler));
    }
}
