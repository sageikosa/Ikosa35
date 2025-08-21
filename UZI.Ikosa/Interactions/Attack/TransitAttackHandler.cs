using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Dice;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Senses;
using System.Collections.ObjectModel;
using Uzi.Visualize;
using System.Diagnostics;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Transits the attack, determines cover, and post processes concealment miss chance
    /// </summary>
    [Serializable]
    public class TransitAttackHandler : IProcessFeedback
    {
        #region private bool WillMissByConcealment(CoverConcealmentResult concealment, AttackInteraction atk, MissChanceAlteration missChance)
        /// <summary>
        /// determines whether the attack will miss when confronted with the designated level of concealment
        /// </summary>
        private bool WillMissByConcealment(IInteract target, CoverConcealmentResult concealment, AttackData atk, ref MissChanceAlteration missChance)
        {
            // get the score for this check
            int _score;

            // add concealment alteration if necessary
            if (!atk.Alterations.OfType<ConcealmentAlteration>().Any())
            {
                atk.Alterations.Add(target, new ConcealmentAlteration(atk, this));
            }

            // make sure there is a miss chance to check with
            if (missChance != null)
            {
                // either the previous score
                _score = missChance.PercentRolled;
            }
            else
            {
                // or a new score, now attached to the attack
                _score = DieRoller.RollDie(target.ID, 100, @"Concealed", @"Miss chance");
                missChance = new MissChanceAlteration(atk, this, _score);
                atk.Alterations.Add(target, missChance);
            }

            if (((concealment == CoverConcealmentResult.Partial) && (_score <= 20))
                || ((concealment == CoverConcealmentResult.Total) && (_score <= 50)))
            {
                // miss (reroll if blindfight)
                if ((atk.Attacker is Creature _critter)
                    && !missChance.SecondRoll
                    && _critter.Feats.Contains(typeof(BlindFight)))
                {
                    _score = DieRoller.RollDie(target.ID, 100, $@"{_critter.Name} BlindFight", @"Miss chance");
                    missChance.SecondChance(_score);
                    if (((concealment == CoverConcealmentResult.Partial) && (_score <= 20))
                        || ((concealment == CoverConcealmentResult.Total) && (_score <= 50)))
                    {
                        // completely missed on the second roll
                        return true;
                    }
                    else
                    {
                        // did not miss on the second roll
                        return false;
                    }
                }
                else
                {
                    // could not make a second roll, and we missed
                    return true;
                }
            }
            else
            {
                // did not miss on the current roll
                return false;
            }
        }
        #endregion

        #region public void ProcessFeedback(InteractWorkSet workSet)
        public void ProcessFeedback(Interaction workSet)
        {
            // if we didn't hit, there's no need to check miss chance
            var _feedback = workSet.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            if (!(_feedback?.Hit ?? false))
            {
                return;
            }

            // already have a miss change that failed to miss for max criteria?
            var _miss = workSet.InteractData.Alterations.OfType<MissChanceAlteration>().FirstOrDefault();
            if ((_miss?.PercentRolled ?? 0) > 50)
            {
                return;
            }

            var _target = workSet.Target as ICoreObject;
            var _atk = workSet.InteractData as AttackData;

            // awareness handling deals with invisibility by having a lesser awareness level
            if (_atk.Attacker is Creature _critter)
            {
                // anything being held has no concealment for attack purposes
                if (_critter.Body.ItemSlots.HeldObjects.Any(_ho => _ho == _target))
                {
                    return;
                }

                if (_critter.Awarenesses.IsTotalConcealmentMiss(_target.ID))
                {
                    // blind attack
                    if (WillMissByConcealment(workSet.Target, CoverConcealmentResult.Total, _atk, ref _miss))
                    {
                        // sketchy awareness, total concealment couldn't hit
                        _feedback.Hit = false;
                        _feedback.CriticalHit = false;
                    }

                    // no more missable than by total concealment
                    //workSet.Feedback.Add(_feedback);
                    return;
                }

                // locate the target in the map environment
                var _map = _target.GetLocated().Locator.Map;
                var _tLoc = Locator.FindFirstLocator(_target);
                var _aLoc = Locator.FindFirstLocator(_critter);
                var _excl = ITacticalInquiryHelper.GetITacticals(_target, _critter).ToArray();
                var _nearDistance = _tLoc.GeometricRegion.NearDistance(_aLoc.GeometricRegion);

                // get best active attacker senses
                var _available = BestSenses(_tLoc, _nearDistance, _critter.Senses.CollectBestSenses());

                // find concealment miss chance and turn the feedback to a miss if necessary; varies by sense
                if (workSet.InteractData is MeleeAttackData _mAtk)
                {
                    if (_available.Any())
                    {
                        var _tCell = _mAtk.TargetCell;

                        // from each corner of the attacker ...
                        var _factory = new SegmentSetFactory(_map, _aLoc.GeometricRegion, _tCell.ToCellPosition(),
                            _excl, SegmentSetProcess.Observation);
                        var _group = _available.GroupBy(_s => _s.PlanarPresence).ToList();
                        foreach (var _startPt in _aLoc.GeometricRegion.AllCorners())
                        {
                            foreach (var _presence in _group)
                            {
                                // ... to each corner of the target cell
                                foreach (var _line in from _end in _tCell.AllCorners()
                                                      select _map.SegmentCells(_startPt, _end, _factory, _presence.Key))
                                {
                                    #region check each sense to see if the sense-line is "perfect" (usable by sense and unconcealed)
                                    // check each sense (since any sense can "negate" melee concealment)
                                    foreach (var _sense in _presence)
                                    {
                                        // ensure sense is in range
                                        if (_line.Vector.Length <= _sense.Range)
                                        {
                                            // line not good enough to consider
                                            if (_sense.UsesLineOfEffect && !_line.IsLineOfEffect)
                                            {
                                                break;
                                            }

                                            // concealment found for concealable sense
                                            if (!_sense.IgnoresConcealment && (_line.SuppliesConcealment() > CoverConcealmentResult.None))
                                            {
                                                break;
                                            }

                                            // found a good sense line that doesn't transit and isn't concealed
                                            if (!_sense.UsesSenseTransit)
                                            {
                                                return;
                                            }

                                            // regen a fresh sense transit (in case of alterations)
                                            var _sTrans = new SenseTransit(_sense);
                                            var _senseSet = new Interaction(workSet.Actor, _sense, workSet.Target, _sTrans);

                                            // see if line really isn't good enough
                                            if (!_line.CarryInteraction(_senseSet))
                                            {
                                                break;
                                            }

                                            // at this point, we have a "perfect" sense-line
                                            return;
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                    }

                    // ran out of senses before lines
                    if (WillMissByConcealment(workSet.Target, CoverConcealmentResult.Partial, _atk, ref _miss))
                    {
                        // sketchy awareness, total concealment couldn't hit
                        _feedback.Hit = false;
                        _feedback.CriticalHit = false;
                    }
                    return;
                }
                else if (workSet.InteractData is ReachAttackData)
                {
                    // reach/ranged effect specific stuff
                    var _rchAtk = workSet.InteractData as ReachAttackData;
                    var _usedSenses = new List<SensoryBase>();

                    if (_available.Any())
                    {
                        // TODO: loop structuring (senses versus lines)

                        // look at each line from attack point ...
                        var _factory = new SegmentSetFactory(_map, _aLoc.GeometricRegion, _tLoc.GeometricRegion,
                            _excl, SegmentSetProcess.Observation);

                        var _senseGroup = _available.GroupBy(_s => _s.PlanarPresence)
                            .Select(_g => (presence: _g.Key, senses: _g.ToList()))
                            .ToList();
                        foreach (var (_presence, _senses) in _senseGroup)
                        {
                            foreach (var _line in _tLoc.GeometricRegion.AllCorners()
                                .Select(_tPt => _map.SegmentCells(_rchAtk.AttackPoint, _tPt, _factory, _presence)))
                            {
                                // ... with each sense (eliminate senses as they prove to be obscured)
                                foreach (var _sense in _senses.ToList())
                                {
                                    // ensure sense is in range
                                    if (_line.Vector.Length <= _sense.Range)
                                    {
                                        if (!_sense.IgnoresConcealment && (_line.SuppliesConcealment() == CoverConcealmentResult.Partial))
                                        {
                                            // a particular sense was concealed, so it is not "left"
                                            _senses.Remove(_sense);
                                            _available.Remove(_sense);
                                        }
                                        else
                                        {
                                            #region Examine lines to ensure unaltered delivery
                                            if (_sense.UsesLineOfEffect)
                                            {
                                                // check line of effect interference
                                                if (_line.IsLineOfEffect)
                                                {
                                                    RangeTestSense(workSet, _usedSenses, _line, _sense);
                                                }
                                            }
                                            else if (!_sense.UsesSenseTransit && _sense.IgnoresConcealment)
                                            {
                                                // targetting sense that doesn't use line of effect or sense transit!
                                                return;
                                            }
                                            else
                                            {
                                                RangeTestSense(workSet, _usedSenses, _line, _sense);
                                            }
                                            #endregion
                                        }
                                    }
                                }

                                // quit examining lines, if we ran out of senses in this group
                                if (!_senses.Any())
                                {
                                    break;
                                }
                            }
                        }
                    }

                    // see if none left, or those that are left were not used
                    if (!_available.Any() || !_available.Intersect(_usedSenses).Any())
                    {
                        // ran out of senses before lines
                        if (WillMissByConcealment(workSet.Target, CoverConcealmentResult.Partial, _atk, ref _miss))
                        {
                            // sketchy awareness, total concealment couldn't hit
                            _feedback.Hit = false;
                            _feedback.CriticalHit = false;
                        }
                        return;
                    }
                }
            }
            else
            {
                // default is to return the existing feedback
                return;
            }
        }
        #endregion

        #region private static List<SensoryBase> BestSenses(Locator targetLocator, double nearDistance, Collection<SensoryBase> bestSoFar)
        /// <summary>Lists the best targeting senses that are not inherently shadowed and which have potential visibility on the target</summary>
        private static List<SensoryBase> BestSenses(Locator targetLocator, double nearDistance, Collection<SensoryBase> bestSoFar)
        {
            #region gather best senses
            // order the best targetting senses
            var _bestLeft = (from _s in bestSoFar
                             where _s.ForTargeting && (_s.Range >= nearDistance)
                             orderby _s.Precedence descending
                             select _s).ToList();

            // exclude senses that have poor visibility
            _bestLeft.ToList().ForEach(_sense =>
            {
                // assume the worst
                var (_visible, _shadowed) = targetLocator.VisibilityForSense(_sense);

                // check only if the sense could possibly determine visibility
                if (!_visible || _shadowed)
                {
                    _bestLeft.Remove(_sense);
                }
            });
            #endregion

            return _bestLeft;
        }
        #endregion

        #region private static void RangeTestSense(Interaction workSet, List<SensoryBase> usedSenses, SegmentSet line, SensoryBase sense)
        /// <summary>Uses senses if they are not destroyed or altered.  Removes senses from further consideration if they are altered</summary>
        private static void RangeTestSense(Interaction workSet, List<SensoryBase> usedSenses, SegmentSet line, SensoryBase sense)
        {
            if (sense.UsesSenseTransit)
            {
                // regen a fresh sense transit (in case of alterations)
                var _sTrans = new SenseTransit(sense);
                var _senseSet = new Interaction(workSet.Actor, sense, workSet.Target, _sTrans);
                if (!line.CarryInteraction(_senseSet))
                {
                    return;
                }
            }

            if (!usedSenses.Contains(sense))
            {
                usedSenses.Add(sense);
            }
        }
        #endregion

        #region public void HandleInteraction(InteractWorkSet workSet)
        public void HandleInteraction(Interaction workSet)
        {
            // must have an attack
            if (workSet.InteractData is AttackData _atk)
            {
                var _target = workSet.Target as ICoreObject;
                var _map = _atk.AttackLocator.Map;
                var _excl = ITacticalInquiryHelper.GetITacticals(_atk.Attacker, _target).ToArray();
                var _atkPresence = _atk.AttackLocator.PlanarPresence;
                if (_target == null)
                {
                    if (_atk is MeleeAttackData _mAtk)
                    {
                        #region melee attack without target (blind, vs. invisible, or grasping)
                        // get source and target points
                        var _srcPts = GetMeleeSourcePoints.GetPoints(_mAtk);
                        var _trgPts = GetMeleeTargetPoints.GetPoints(_mAtk, workSet.Actor, _srcPts.DownWard, _srcPts.DownFace); // NOTE: using actor to handle

                        // get cover bonus
                        var _cover = _map.CoverValue(_srcPts.Value, _trgPts.Value,
                            _mAtk.SourceCell.ToCellPosition(), _mAtk.TargetCell.ToCellPosition(), false, workSet, _atkPresence, _excl);
                        if (_cover == Int32.MaxValue)
                        {
                            // either no effect lines, or all are incapable of carrying
                            workSet.Feedback.Add(new AttackFeedback(this, false) { NoLines = true });
                            return;
                        }
                        else if (_cover > 0)
                        {
                            // if real cover, add an attack alteration
                            workSet.InteractData.Alterations.Add(workSet.Target, new CoverAlteration(_atk, _cover));
                        }

                        // must miss, no target
                        workSet.Feedback.Add(new AttackFeedback(this, false));
                        #endregion
                    }
                    else if ((_atk is ReachAttackData _rchAtk) && (_atk.TargetCell != null))
                    {
                        #region reach attack without target
                        // get points and lines

                        var _srcPts = _rchAtk.AttackPoint.ToEnumerable().ToList();
                        var _trgPts = _rchAtk.TargetCell.AllCorners().ToList();
                        var _lines = _map.GetLinesSets(_srcPts, _trgPts, _rchAtk.SourceCell.ToCellPosition(),
                            _rchAtk.TargetCell.ToCellPosition(), _atkPresence, _excl);
                        var _maxCount = _lines.Count;

                        // good lines are effect lines that can carry the interaction
                        var _goodLines = _lines.AsParallel()
                            .Where(_l => _l.IsLineOfEffect && _l.CarryInteraction(workSet))
                            .ToList();

                        // get cover value for the attack-type
                        var _cover = _map.CoverValue(_goodLines, _maxCount, true, workSet);
                        if (_cover == Int32.MaxValue)
                        {
                            // either no effect lines, or all are incapable of carrying
                            workSet.Feedback.Add(new AttackFeedback(this, false) { NoLines = true });
                            return;
                        }
                        else if (_cover > 0)
                        {
                            // if real cover, add an attack alteration
                            workSet.InteractData.Alterations.Add(workSet.Target, new CoverAlteration(_atk, _cover));
                        }

                        // must miss (no target)
                        workSet.Feedback.Add(new AttackFeedback(this, false));
                        #endregion
                    }
                }
                else
                {
                    // map and locator for target
                    var _tLoc = Locator.FindFirstLocator(_target);

                    // get cover alterations
                    if (workSet.InteractData is MeleeAttackData _mAtk)
                    {
                        // if cells are identical, no need to check lines
                        var _equality = new CellLocationEquality();
                        if (!_equality.Equals(_mAtk.SourceCell, _mAtk.TargetCell))
                        {
                            #region melee cover and downward
                            var _srcPts = GetMeleeSourcePoints.GetPoints(_mAtk);
                            var _trgPts = GetMeleeTargetPoints.GetPoints(_mAtk, workSet.Target, _srcPts.DownWard, _srcPts.DownFace);

                            // get cover bonus
                            var _cover = _map.CoverValue(_srcPts.Value, _trgPts.Value, _mAtk.SourceCell.ToCellPosition(),
                                _mAtk.TargetCell.ToCellPosition(), false, workSet, _atkPresence, _excl);
                            if (_cover == Int32.MaxValue)
                            {
                                // either no effect lines, or all are incapable of carrying
                                workSet.Feedback.Add(new AttackFeedback(this, false) { NoLines = true });
                                return;
                            }
                            else if (_cover > 0)
                            {
                                // if real cover, add an attack alteration
                                workSet.InteractData.Alterations.Add(workSet.Target, new CoverAlteration(_atk, _cover));
                            }

                            if (!_atkPresence.HasOverlappingPresence(_tLoc.PlanarPresence))
                            {
                                // must miss, incompatible contexts
                                workSet.Feedback.Add(new AttackFeedback(this, false));
                                return;
                            }

                            // downward
                            if (_trgPts.DownWard)
                            {
                                workSet.InteractData.Alterations.Add(workSet.Target, new HigherGroundAlteration(_atk, 1));
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        // TODO: engaged in melee...

                        #region Reach/Ranged Cover and Downward
                        // ranged attack handling includes reach weapons
                        if (workSet.InteractData is ReachAttackData _rchAtk)
                        {
                            // must be located
                            var _located = (workSet.Target as IAdjunctable)?.GetLocated();
                            if (_located != null)
                            {
                                // if the attack cannot be carried through the environment, fail it
                                var _trgPts = GetRangedTargetPoints.GetPoints(_rchAtk, workSet.Target);
                                var _srcPts = _rchAtk.AttackPoint.ToEnumerable().ToList();

                                // see if source is inside target
                                if (!_trgPts.Value.Any())
                                {
                                    // region must contain sourceCell
                                    var _region = _located.Locator.GeometricRegion;
                                    if (_region.ContainsCell(_rchAtk.SourceCell))
                                    {
                                        // no cover, no range increment, no downward
                                        return;
                                    }
                                }

                                var _lines = _map.GetLinesSets(_srcPts, _trgPts.Value,
                                    _rchAtk.SourceCell.ToCellPosition(), _located.Locator.GeometricRegion, _atkPresence, _excl);
                                var _maxCount = _lines.Count;

                                // good lines are effect lines that can carry the interaction
                                var _goodLines = _lines.AsParallel()
                                    .Where(_l => _l.IsLineOfEffect && _l.CarryInteraction(workSet))
                                    .ToList();

                                // get cover value for the attack-type
                                var _cover = _map.CoverValue(_goodLines, _maxCount, true, workSet);
                                if (_cover == Int32.MaxValue)
                                {
                                    // either no effect lines, or all are incapable of carrying
                                    workSet.Feedback.Add(new AttackFeedback(this, false) { NoLines = true });
                                    return;
                                }
                                else if (_cover > 0)
                                {
                                    // if real cover, add an attack alteration
                                    workSet.InteractData.Alterations.Add(workSet.Target, new CoverAlteration(_atk, _cover));
                                }

                                if (!_atkPresence.HasOverlappingPresence(_tLoc.PlanarPresence))
                                {
                                    // must miss, incompatible contexts
                                    workSet.Feedback.Add(new AttackFeedback(this, false));
                                    return;
                                }

                                // range increment penalties
                                if (_rchAtk is RangedAttackData)
                                {
                                    var _rAtk = _rchAtk as RangedAttackData;
                                    var _nearest = _goodLines.OrderBy(_l => _l.Vector.Length).FirstOrDefault();
                                    if ((_nearest != null) && (_nearest.Vector.Length > Convert.ToDouble(_rAtk.RangedSource.RangeIncrement)))
                                    {
                                        var _rAlt = new RangeIncrementAlteration(_rAtk, Convert.ToInt32(_nearest.Vector.Length));
                                        workSet.InteractData.Alterations.Add(workSet.Target, _rAlt);
                                    }
                                }

                                // downward
                                if (_trgPts.DownWard)
                                {
                                    workSet.InteractData.Alterations.Add(workSet.Target, new HigherGroundAlteration(_atk, 1));
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            return;
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(MeleeAttackData);
            yield return typeof(ReachAttackData);
            yield return typeof(RangedAttackData);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // always want the environment transit handler before any other creature intrinsic handlers
            return true;
        }
        #endregion
    }
}