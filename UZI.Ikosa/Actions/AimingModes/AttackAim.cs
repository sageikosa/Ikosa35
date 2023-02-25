using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Aim mode indicating need to make an attack or ranged attack
    /// </summary>
    [Serializable]
    public class AttackAim : RangedAim, IEnumerable<ITargetType>
    {
        #region construction
        public AttackAim(string key, string displayName, AttackImpact impact, Lethality lethality,
            bool lethalAlternatePenalty, int criticalStart, IRangedSourceProvider provider,
            Range minModes, Range maxModes, Range range, params ITargetType[] validTargetType)
            : base(key, displayName, minModes, maxModes, range)
        {
            Impact = impact;
            CriticalThreatStart = criticalStart;
            ValidTargetTypes = validTargetType ?? new ITargetType[] { };
            RangedSourceProvider = provider;
            LethalOption = lethality;
            HasLethalAlternatePenalty = lethalAlternatePenalty;
        }
        #endregion

        public AttackImpact Impact { get; private set; }
        public int CriticalThreatStart { get; private set; }
        public ITargetType[] ValidTargetTypes { get; private set; }
        public IRangedSourceProvider RangedSourceProvider { get; private set; }

        /// <summary>Indicates to client to aim with a target cell so that indirect secondary effects have a capture zone</summary>
        public bool UseCellForIndirect { get; set; }

        public bool UseHiddenRolls { get; set; }

        public Lethality LethalOption { get; set; }
        public bool HasLethalAlternatePenalty { get; set; }

        #region IEnumerable<TargetType> Members
        public IEnumerator<ITargetType> GetEnumerator()
        {
            foreach (ITargetType _tType in ValidTargetTypes)
            {
                yield return _tType;
            }
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (ITargetType _tType in ValidTargetTypes)
            {
                yield return _tType;
            }
        }
        #endregion

        #region private Delta AlternateChoicePenalty(AttackTargetInfo attack)
        /// <summary>
        /// Provides a -4 delta if an alternate lethal choice is supplied and the attack should apply a penalty
        /// </summary>
        private Delta AlternateChoicePenalty(AttackTargetInfo attack)
        {
            if (HasLethalAlternatePenalty)
                switch (LethalOption)
                {
                    case Lethality.NormallyLethal:
                        if (attack.IsNonLethal)
                            return new Delta(-4, typeof(Lethality), @"Non-lethal attack");
                        break;

                    case Lethality.NormallyNonLethal:
                        if (!attack.IsNonLethal)
                            return new Delta(-4, typeof(Lethality), @"Lethal attack");
                        break;
                }
            return null;
        }
        #endregion

        private IGeometricRegion TargetRegion(IInteract target)
            => (target as IAdjunctable)?.GetLocated()?.Locator?.GeometricRegion;

        #region public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, Core.Services.AimTargetInfo[] infos, IInteractProvider provider)
        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action,
            AimTargetInfo[] infos, IInteractProvider provider)
        {
            var _aLoc = actor.GetLocated().Locator;

            // get targets
            var _allTargets = (from _a in SelectedTargets<AttackTargetInfo>(actor, action, infos)
                               where Impact == _a.Impact
                               select _a).ToList();
            var _harmless = (action as ActionBase)?.IsHarmless ?? true;

            // step through
            var _index = 1;
            foreach (var _atk in _allTargets)
            {
                var _sensors = actor as ISensorHost;
                var _iTarget = provider.GetIInteract(_atk.TargetID ?? Guid.Empty);
                var _tPlanar = (_iTarget as IAdjunctSet)?.GetLocated()?.Locator?.PlanarPresence ?? PlanarPresence.None;

                // score
                var _selfAttended = ((_iTarget as IAdjunctable)?.Adjuncts.OfType<Attended>()
                    .Any(_a => _a.Creature.ID == actor.ID && _a.IsActive) ?? false);
                var _score = _selfAttended
                    ? new Deltable(20)
                    : _atk.GetAttackScoreDeltable(_sensors.ID);
                _score.Deltas.Add(AlternateChoicePenalty(_atk));

                var _trgCell = _atk.GetTargetCell(_sensors, _iTarget);
                var _srcCell = _sensors.GetAimCell(_aLoc.GeometricRegion);
                if (_srcCell != null)
                {
                    var _action = action as ActionBase;
                    if ((Range is MeleeRange) || (Range is StrikeZoneRange))
                    {
                        // melee or reach?
                        // which is it?
                        if (_srcCell.IsCellTouchingBounds(_trgCell))
                        {
                            // melee!
                            var (_iInteract, _location) = FinalizeMeleeTarget(_iTarget, actor, _srcCell, _trgCell, _aLoc, _tPlanar);
                            var _nonLethal = ((LethalOption == Lethality.NormallyLethal) || (LethalOption == Lethality.NormallyNonLethal))
                                ? _atk.IsNonLethal
                                : LethalOption == Lethality.AlwaysNonLethal;

                            #region target is adjacent, use melee attack
                            // melee range
                            if (CriticalThreatStart <= _score.BaseValue)
                            {
                                // criticality
                                var _critScore = _selfAttended
                                    ? new Deltable(20)
                                    : _atk.GetCriticalConfirmDeltable(_sensors.ID);
                                _critScore.Deltas.Add(AlternateChoicePenalty(_atk));
                                yield return new AttackTarget(Key, _iInteract,
                                    new MeleeAttackData(actor, _action, _aLoc, _atk.Impact, _score, _critScore,
                                        _harmless, _srcCell, _location, _index, _allTargets.Count)
                                    {
                                        // if "normally": use attack option, otherwise use always option
                                        IsNonLethal = _nonLethal
                                    });
                            }
                            else
                            {
                                // non-critical
                                yield return new AttackTarget(Key, _iInteract,
                                    new MeleeAttackData(actor, _action, _aLoc, _atk.Impact, _score,
                                        _harmless, _srcCell, _location, _index, _allTargets.Count)
                                    {
                                        // if "normally": use attack option, otherwise use always option
                                        IsNonLethal = _nonLethal
                                    });
                            }
                            #endregion
                        }
                        else
                        {
                            // reach!
                            var _rAtk = GetRangedAttack(false, _action, actor, _aLoc, _atk, _score, _iTarget, _tPlanar, _srcCell, _trgCell, _index, _allTargets.Count);
                            if (_rAtk != null)
                                yield return _rAtk;
                        }
                    }
                    else
                    {
                        // ranged!
                        var _rAtk = GetRangedAttack(true, _action, actor, _aLoc, _atk, _score, _iTarget, _tPlanar, _srcCell, _trgCell, _index, _allTargets.Count);
                        if (_rAtk != null)
                            yield return _rAtk;
                    }
                }

                // increase index counter
                _index++;
            }
            yield break;
        }
        #endregion

        #region private (IInteract iInteract, ICellLocation location) FinalizeMeleeTarget(IInteract target, CoreActor actor, ICellLocation sourceCell, ICellLocation targetCell, LocalMap map)
        /// <summary>If attacking without a target, see if a target can be found at the location</summary>
        private (IInteract iInteract, ICellLocation location) FinalizeMeleeTarget(IInteract target, CoreActor actor,
            ICellLocation sourceCell, ICellLocation targetCell, Locator atkLocator, PlanarPresence trgPlanar)
        {
            var _planar = atkLocator.PlanarPresence;
            if (target != null)
            {
                // target and actor agree on ethereal state
                if (_planar.HasOverlappingPresence(trgPlanar))
                {
                    return (target, targetCell);
                }
            }

            IEnumerable<ICoreObject> _getCapturables(ICoreObject source)
            {
                if (source is ICapturePassthrough _pass)
                {
                    foreach (var _o in _pass.Contents.OfType<ICoreObject>())
                    {
                        yield return _o;
                    }
                }
                else
                {
                    yield return source;
                }
                yield break;
            }

            if (actor is ISensorHost _sensors)
            {
                // see if a blocker that the sensors are unaware of gets in the way...
                var _blocker = (from _mb in atkLocator.Map.MeleeBlockers(sourceCell, targetCell, _planar, actor)
                                from _capt in _getCapturables(_mb.CoreObj)
                                let _aware = _sensors.Awarenesses[_capt.ID]
                                orderby _aware descending
                                select (iInteract: _capt, location: _mb.Location))
                               .FirstOrDefault();
                if (_blocker.iInteract != null)
                    return _blocker;
            }

            // NOTE: perhaps a better selection mechanic?
            // get first locator's first object (excluding actor)
            return ((from _loc in atkLocator.MapContext.LocatorsInCell(targetCell, _planar)
                     from _o in _loc.GetCapturable<IInteract>()
                     where _o != actor
                     select _o).FirstOrDefault(), targetCell);
        }
        #endregion

        #region private IInteract RangedTryEnsureIInteract(CoreActor actor, IInteract target, Point3D attackPt, ICellLocation location, LocalMap map)
        /// <summary>If attacking without a target, see if the attack line runs into something</summary>
        private IInteract RangedTryEnsureIInteract(CoreActor actor, IInteract target, Point3D attackPt, ICellLocation location,
            Locator atkLoc, PlanarPresence trgPlanar)
        {
            var _planar = atkLoc.PlanarPresence;
            if ((target != null) && _planar.HasOverlappingPresence(trgPlanar))
            {
                // target and actor agree on ethereal state
                return target;
            }

            // direct line from attackPoint to location (intersection)
            var _excl = ITacticalInquiryHelper.GetITacticals(actor).ToArray();
            var _aLoc = actor.GetLocated()?.Locator;
            var _factory = new SegmentSetFactory(atkLoc.Map, _aLoc?.GeometricRegion, location.ToCellPosition(), _excl,
                SegmentSetProcess.Effect);
            var _line = atkLoc.Map.SegmentCells(attackPt, location.Point3D(), _factory, _planar);
            if (_line.BlockedObject is ICapturePassthrough _pass)
            {
                return _pass.Contents.FirstOrDefault() as IInteract;
            }
            else
            {
                return _line.BlockedObject as IInteract;
            }
        }
        #endregion

        #region private AimTarget GetRangedAttack(ActionBase action, CoreActor actor, Locator actorLoc, AttackTargetInfo attack, Deltable score, IInteract iTarget, ICellLocation targetCell)
        /// <summary>
        /// common target conversion for reach and ranged attack
        /// </summary>
        private AimTarget GetRangedAttack(bool ranged, ActionBase rangedAction, CoreActor actor, Locator actorLoc,
            AttackTargetInfo attack, Deltable score, IInteract iTarget, PlanarPresence trgPlanar,
            ICellLocation sourceCell, ICellLocation targetCell, int targetIndex, int targetCount)
        {
            // reach!
            var _sensors = actor as ISensorHost;
            var _atkPt = _sensors.AimPoint;

            // if "normally": use attack option, otherwise use always option
            var _nonLethal = ((LethalOption == Lethality.NormallyLethal) || (LethalOption == Lethality.NormallyNonLethal))
                ? attack.IsNonLethal
                : LethalOption == Lethality.AlwaysNonLethal;

            iTarget = RangedTryEnsureIInteract(actor, iTarget, _atkPt, targetCell, actorLoc, trgPlanar);

            if ((iTarget is Creature _tCritter)
                && _tCritter.Conditions.Contains(Condition.Grappling)
                && !((actor as Creature)?.Feats.Contains(typeof(ImprovedPreciseShotFeat)) ?? false))
            {
                // if target is in a grapple, randomize target amongst all grapplers of target
                var _grapplers = _tCritter.Adjuncts.OfType<Grappler>()
                    .SelectMany(_g => _g.GrappleGroup.Grapplers)
                    .Select(_g => _g.Creature)
                    .Where(_c => _c != null)
                    .Distinct()
                    .ToList();
                var _idx = DieRoller.RollDie(actor.ID, (byte)_grapplers.Count, @"Grappler", @"Randomly select target from grapplers");
                iTarget = _grapplers[_idx - 1];
            }

            var _projectile = RangedSourceProvider.GetRangedSource(actor, rangedAction, this, iTarget);
            var _trgRegion = TargetRegion(iTarget);
            AttackData _atkData;

            // reach geometry (could be a null line, which is OK)
            if (CriticalThreatStart <= score.BaseValue)
            {
                // criticality
                var _critScore = attack.GetCriticalConfirmDeltable(_sensors.ID);
                _critScore.Deltas.Add(AlternateChoicePenalty(attack));
                if (ranged)
                {
                    // max range check
                    var _max = Range.EffectiveRange(actor, rangedAction.CoreActionClassLevel(actor, this));
                    _atkData = new RangedAttackData(actor, _projectile, rangedAction, actorLoc, attack.Impact, score, _critScore,
                        rangedAction.IsHarmless, _atkPt, sourceCell, targetCell, targetIndex, targetCount)
                    {
                        IsNonLethal = _nonLethal,
                        InRange = IGeometricHelper.NearDistance(_trgRegion, _atkPt) <= _max
                    };
                }
                else
                {
                    // zone check
                    var _inZone = (Range as StrikeZoneRange).Zone.ContainsGeometricRegion(_trgRegion, iTarget as ICoreObject, trgPlanar);
                    _atkData = new ReachAttackData(actor, rangedAction, actorLoc, attack.Impact, score, _critScore,
                        rangedAction.IsHarmless, _atkPt, sourceCell, targetCell, targetIndex, targetCount)
                    {
                        IsNonLethal = _nonLethal,
                        InRange = _inZone
                    };
                }
            }
            else
            {
                // non-critical
                if (ranged)
                {
                    // max range check
                    var _max = Range.EffectiveRange(actor, rangedAction.CoreActionClassLevel(actor, this));
                    _atkData = new RangedAttackData(actor, _projectile, rangedAction, actorLoc, attack.Impact, score,
                        rangedAction.IsHarmless, _atkPt, sourceCell, targetCell, targetIndex, targetCount)
                    {
                        IsNonLethal = _nonLethal,
                        InRange = IGeometricHelper.NearDistance(_trgRegion, _atkPt) <= _max
                    };
                }
                else
                {
                    // zone check
                    var _inZone = (Range as StrikeZoneRange).Zone.ContainsGeometricRegion(_trgRegion, iTarget as ICoreObject, trgPlanar);
                    _atkData = new ReachAttackData(actor, rangedAction, actorLoc, attack.Impact, score, rangedAction.IsHarmless,
                        _atkPt, sourceCell, targetCell, targetIndex, targetCount)
                    {
                        IsNonLethal = _nonLethal,
                        InRange = _inZone
                    };
                }
            }

            return new AttackTarget(Key, iTarget, _atkData);
        }
        #endregion

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToRangedAimInfo<AttackAimInfo>(action, actor);

            _info.CriticalThreatStart = CriticalThreatStart;
            _info.ValidTargetTypes = ValidTargetTypes.Select(_vtt => _vtt.ToTargetTypeInfo()).ToArray();
            _info.Impact = Impact;
            _info.UseCellForIndirect = UseCellForIndirect;
            _info.LethalOption = LethalOption;
            _info.UseHiddenRolls = UseHiddenRolls;
            return _info;
        }
    }
}
