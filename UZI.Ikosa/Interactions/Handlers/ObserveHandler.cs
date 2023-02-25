using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using System.Linq;
using System.Diagnostics;
using System.Text;
using Uzi.Visualize;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Adjuncts;
using System.Runtime.InteropServices;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Every observable (ie, real) object implements this handler.
    /// Only pure virtual objects (such as point in space spell effects) do not.
    /// </summary>
    [Serializable]
    public class ObserveHandler : IInteractHandler
    {
        #region public void HandleInteraction(Interaction workSet)
        public virtual void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is Observe _obs)
            {
                if (workSet.Target is CoreObject _target
                    && _obs.Viewer is ISensorHost _viewer
                    && _viewer.IsSensorHostActive)
                {
                    var _targetLocator = _obs.GetTargetLocator(_target);
                    var _observerLocator = _obs.ObserverLocator;
                    if (_viewer.ID == _target.ID)
                    {
                        // viewer is aware of self...
                        workSet.Feedback.Add(new ObserveFeedback(this, new KeyValuePair<Guid, AwarenessLevel>[]
                        {
                            new KeyValuePair<Guid, AwarenessLevel>(_target.ID, AwarenessLevel.Aware)
                        }));
                        return;
                    }

                    // TODO: ensure target and viewer share compatible contexts (not just first of default)
                    var _distance = _obs.GetDistance(_target);

                    // check for active invisibility
                    var _invisActive = !(_target as IVisible).IsVisible;

                    // gather senses
                    var _best = _viewer.Senses.BestSenses.Where(_s => _s.ForTargeting).ToList();

                    // have at least one sense that uses line of effect in range 
                    // ... and invisibility is not a factor
                    var _effectBest = _best
                        .Where(_b => _b.UsesLineOfEffect
                            && (_b.Range >= _distance)
                            && (!_invisActive || (_invisActive && _b.IgnoresInvisibility)))
                        .ToList();
                    if (_effectBest.Any())
                    {
                        var _process = ProcessAllLinesOfEffect(workSet, _observerLocator, _targetLocator, _effectBest, _target, _viewer);
                        if (_process == null)
                        {
                            // dark-visible
                            workSet.Feedback.Add(
                                new ObserveFeedback(this, new KeyValuePair<Guid, AwarenessLevel>[]
                                {
                                    new KeyValuePair<Guid, AwarenessLevel>(_target.ID, AwarenessLevel.DarkDraw)
                                }));
                            return;
                        }
                        if (_process ?? true)
                            return;
                    }
                    //else
                    //{
                    //    Debug.WriteLine($@"Observe: {(workSet.Source as Creature)?.Name ?? "NULL"} => {_target.Name}: _best.Count() = {_best.Count}");
                    //    Debug.WriteLine($@"Observe: {(workSet.Source as Creature)?.Name ?? "NULL"} => {_target.Name}: _distance     = {_distance}");
                    //    Debug.WriteLine($@"Observe: {(workSet.Source as Creature)?.Name ?? "NULL"} => {_target.Name}: _invisActive  = {_invisActive}");
                    //    Debug.WriteLine($@"Observe: {(workSet.Source as Creature)?.Name ?? "NULL"} no-senses {_target.Name}");
                    //}

                    // TODO: non-effect-based senses

                    // couldn't get an observation line that worked
                    workSet.Feedback.Add(
                        new ObserveFeedback(this, new KeyValuePair<Guid, AwarenessLevel>[]
                        {
                            new KeyValuePair<Guid, AwarenessLevel>(_target.ID, AwarenessLevel.None)
                        }));
                }
            }
            return;
        }
        #endregion

        #region protected virtual bool? ProcessAllLinesOfEffect(...)
        /// <summary>
        /// default behavior is to get all observation lines (line of effect) between regions,
        /// preventing the target object from providing cover to itself, then checking each sense
        /// </summary>
        protected virtual bool? ProcessAllLinesOfEffect(Interaction workSet, Locator observerLocator,
            Locator targetLocator, List<SensoryBase> senses, ICoreObject target, ISensorHost sensors)
        {
            // get all lines of effect
            var _darkVisible = false;

            // TODO: improve performance

            //var _lTypes = sensors?.PathHasActiveAdjunct<Ethereal>() ? 
            // NOTE: when ethereal, blindsight ignores all non-ethereal, but vision sees all
            var _maxDist = sensors?.Skills.Skill<SpotSkill>().MaxMultiDistance ?? 100;
            var _factory = new SegmentSetFactory(observerLocator.Map,
                observerLocator.GeometricRegion, targetLocator.GeometricRegion,
                ITacticalInquiryHelper.GetITacticals(sensors, target).ToArray(), SegmentSetProcess.Observation);

            foreach (var _presence in GetDistinctPlanarPresences(senses))
            {
                if (_presence.HasOverlappingPresence(targetLocator.PlanarPresence))
                {
                    foreach (var _lSet in observerLocator.LinesToTarget(targetLocator.GeometricRegion, _factory, _maxDist, _presence)
                        .Where(_l => _l.IsLineOfEffect))
                    {
                        var _check = CheckSenses(workSet, _lSet, senses, target, targetLocator.LightLevel, _presence);
                        if (_check ?? false)
                        {
                            return true;
                        }
                        if (_check == null)
                            _darkVisible = true;
                    }
                }
            }

            // if it should be visible, but not enough light, then it is dark-visible
            return _darkVisible ? (bool?)null : false;
        }
        #endregion

        protected IEnumerable<PlanarPresence> GetDistinctPlanarPresences(List<SensoryBase> senses)
            => senses.Select(_s => _s.PlanarPresence).Distinct();

        #region protected bool? CheckSenses(Interaction workSet, SegmentSet obsSet, List<SensoryBase> senses, CoreObject target, LightRange lightLevel)
        protected bool? CheckSenses(Interaction workSet, SegmentSet obsSet, List<SensoryBase> senses,
            ICoreObject target, LightRange lightLevel, PlanarPresence planar)
        {
            // check each sense
            var _darkVisible = false;
            foreach (var _sense in senses.Where(_s => _s.PlanarPresence == planar))
            {
                // regen a fresh sense transit (in case of alterations)
                var _sTrans = new SenseTransit(_sense);
                var _senseSet = new Interaction(null, _sense, workSet.Target, _sTrans);

                // pass each line-of-effect through the environment, looking for alterations
                // NOTE: a single total concealment will block a line
                //       but not prevent all awareness of cell or locator
                var _covCon = _sense.IgnoresConcealment
                    ? CoverConcealmentResult.None
                    : obsSet.SuppliesConcealment(); // target exclusion
                if (_covCon < CoverConcealmentResult.Total)
                {
                    if (!_sense.UsesSenseTransit || obsSet.CarryInteraction(_senseSet))
                    {
                        // at least one that makes it (not snuffed by total concealment or silence)...
                        if (_sense.WorksInLightLevel(lightLevel))
                        {
                            workSet.Feedback.Add(BuildFeedback(_sense, target, AwarenessLevel.Aware));
                            return true;
                        }
                        else
                        {
                            _darkVisible = true;
                        }
                    }
                }
            }

            // if it should be visible, but not enough light, then it is dark-visible
            return _darkVisible ? (bool?)null : false;
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(Observe);
            yield return typeof(SearchData);
            yield break;
        }
        #endregion

        #region public virtual bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public virtual bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
        #endregion

        #region private ObserveFeedback BuildFeedback(SensoryBase sense, CoreObject mainTarget, AwarenessLevel calculatedLevel)
        private ObserveFeedback BuildFeedback(SensoryBase sense, ICoreObject mainTarget, AwarenessLevel calculatedLevel)
        {
            var _retVal = new Dictionary<Guid, AwarenessLevel>
            {
                { mainTarget.ID, calculatedLevel }
            };
            if (calculatedLevel == AwarenessLevel.Aware)
            {
                foreach (var _iVis in mainTarget.Adjuncts.OfType<IVisible>())
                {
                    if (_iVis.IsVisible)
                    {
                        // visible
                        if (sense.UsesSight)
                        {
                            _retVal.Add(_iVis.ID, calculatedLevel);
                        }
                        else
                        {
                            // all sight senses have high precedence, so if we are scraping bottom, not aware
                            _retVal.Add(_iVis.ID, AwarenessLevel.None);
                        }
                    }
                    else
                    {
                        // invisible and at least one detecter is known to be operating
                        if (sense.UsesSight && sense.IgnoresInvisibility)
                        {
                            // the sense that picked up the target can see it
                            _retVal.Add(_iVis.ID, calculatedLevel);
                        }
                        else
                        {
                            // all invis ignoring sight senses have high precedence, so if we are scraping bottom, not aware
                            _retVal.Add(_iVis.ID, AwarenessLevel.None);
                        }
                    }
                }
            }
            return new ObserveFeedback(this, _retVal);
        }
        #endregion
    }
}
