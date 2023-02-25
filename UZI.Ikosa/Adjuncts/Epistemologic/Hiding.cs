using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Hiding : Adjunct, IProcessFeedback
    {
        public Hiding(Deltable baseDifficulty)
            : base(baseDifficulty)
        {
        }

        public Deltable Difficulty => (Deltable)Source;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            (Anchor as CoreObject)?.AddIInteractHandler(this);
            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            (Anchor as CoreObject)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }
        #endregion

        #region protected virtual bool CanHide(Locator observer, Locator target, IVisible iVisible, SensoryBase sense)
        /// <summary>See whether the target can hide from the observer, using the given sense.</summary>
        protected virtual bool CanHide(ISensorHost observer, Locator obsLocator, Locator targetLocator, IVisible iVisible, SensoryBase sense)
        {
            var _distance = targetLocator.GeometricRegion.NearDistance(obsLocator.GeometricRegion);

            // check for active invisibility on all locatable objects on the locator
            var _invisActive = !iVisible.IsVisible;

            // invisible and observer ignoring invisiblity, or not invisibile; AND within range
            if (((_invisActive && sense.IgnoresInvisibility) || !_invisActive) && (sense.Range >= _distance))
            {
                // assume not lit and shrouded in normal shadows or magical darkness
                var (_litUp, _shadowShroud) = targetLocator.VisibilityForSense(sense);

                // if shrouded in shadows, can hide from this sense
                if (_shadowShroud)
                    return true;

                if (!sense.UsesLight || (sense.UsesLight && _litUp))
                {
                    foreach (var _iSect in obsLocator.GeometricRegion.AllCorners())
                    {
                        // if unobscured from a point, then cannot hide
                        if (!targetLocator.IsObscuredFromObserverPoint(observer, _iSect, sense))
                            return false;
                    }
                }
            }
            return true;
        }
        #endregion

        // TODO: actions that terminate hiding

        public override object Clone()
        {
            return new Hiding(Difficulty);
        }

        #region IProcessFeedback Members

        public void ProcessFeedback(Interaction workSet)
        {
            if (workSet.InteractData is Observe _obs)
            {
                if ((workSet.Target is CoreObject _target)
                    && (_obs.Viewer is ISensorHost _viewer)
                    && _viewer.IsSensorHostActive)
                {
                    // if viewer hasn't NO awareness, then a spot may be attemptable
                    if ((_viewer?.Awarenesses?.GetAwarenessLevel(_target.ID) ?? AwarenessLevel.None) == AwarenessLevel.None)
                    {
                        // would observe feedback expose the hiding target?
                        var _obsBack = workSet.Feedback
                            .OfType<ObserveFeedback>()
                            .FirstOrDefault(_of =>
                                _of.Levels.ContainsKey(_target.ID) &&
                                _of.Levels[_target.ID] == AwarenessLevel.Aware);
                        if (_obsBack != null)
                        {
                            // can target hide from observer (cover, concealment, etc.)
                            var _targetLocator = _obs.GetTargetLocator(_target);
                            var _observerLocator = _obs.ObserverLocator;
                            if (!_viewer.Senses.BestSenses
                                .Any(_s => CanHide(_viewer, _observerLocator, _targetLocator, _target as IVisible, _s)))
                                return;

                            // auto-make spot check
                            var _distance = _obs.GetDistance(_target);
                            var _hideDC = Difficulty.EffectiveValue + Convert.ToInt32(_distance / 10);
                            if (!_viewer.Skills.Skill<SpotSkill>().AutoCheck(_hideDC, _target))
                            {
                                // failed spot, makes unaware (not none)
                                _obsBack.Levels[_target.ID] = AwarenessLevel.UnAware;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(Observe);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return !(existingHandler is Searchable);
        }

        #endregion
    }
}
