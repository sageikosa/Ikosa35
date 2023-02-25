using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Interactions;
using System;

namespace Uzi.Ikosa.Tactical
{
    public interface ILightTarget : IGeometricContext
    {
        /// <summary>Indicates whether the target is in an area of deep shadows</summary>
        ShadowModel ShadowModel { get; }
    }

    public enum ShadowModel
    {
        Deep = -1,
        Mixed = 0,
        Normal = 1
    }

    public interface ILightVisibleTarget : ILightTarget
    {
        /// <summary>Tracks whether the target is in an area of magical darkness</summary>
        bool InMagicalDarkness { get; set; }

        void SetLightLevel(LightRange range);

        /// <summary>Light Level calculated at the target</summary>
        LightRange LightLevel { get; }

        /// <summary>Enumerates objects at the target (for interaction purposes)</summary>
        IEnumerable<CoreObject> IlluminableObjects { get; }

        /// <summary>Gets the illuminator determined to best light up the target</summary>
        IIllumination BestIlluminator { get; set; }
    }

    /// <summary>Provides mechanisms to determine light level for light targets (Locators)</summary>
    public static class LightVisibleTargetStatics
    {
        #region public static IEnumerable<IIllumination> GetIlluminators(this ILightVisibleTarget target)
        /// <summary>Get illuminators that might affect this object</summary>
        public static IEnumerable<IIllumination> GetIlluminators(this ILightVisibleTarget target)
        {
            bool _backgroundGroup = false;
            foreach (var _group in target.GetLocalCellGroups())
            {
                if (!_group.IsPartOfBackground)
                {
                    foreach (var _illum in _group.GetIlluminators(false))
                    {
                        yield return _illum;
                    }
                }
                else
                {
                    // NOTE: separated, since effects in background include all backgrounds
                    if (!_backgroundGroup)
                    {
                        // background illuminations (no links between background, so get them all)
                        foreach (var _illum in _group.GetIlluminators(false))
                            yield return _illum;

                        // flag to process backgrounds only once
                        _backgroundGroup = true;
                    }

                }

                // ambient illuminations
                foreach (var _light in _group.GetAmbientIlluminators())
                {
                    yield return _light;
                }
            }
            yield break;
        }
        #endregion

        #region public static void DetermineIllumination(this ILightVisibleTarget target)
        /// <summary>Locator determines LightLevel based upon the effects in the environment</summary>
        public static void DetermineIllumination(this ILightVisibleTarget self)
        {
            #region Darkness Shrouded Effects
            // Darkness Shrouded Effects and Light-Bathed overrides
            // NOTE: a LocalLink will not care about darkness shrouded
            // ... as it needs the best light to pass through beyond it anyway
            var _darkest = (from _base in self.IlluminableObjects
                            from _shroud in _base.Adjuncts.OfType<DarknessShrouded>()
                            let _powerLevel = (_shroud.Source as MagicPowerEffect)?.PowerLevel ?? 0
                            orderby _powerLevel descending
                            select new
                            {
                                Shroud = _shroud,
                                Level = _powerLevel
                            }).FirstOrDefault();

            if (_darkest != null)
            {
                // so look for light bathed effects
                var _brightest = (from _base in self.IlluminableObjects
                                  from _light in _base.Adjuncts.OfType<LightBathed>()
                                  let _powerLevel = (_light.Source as MagicPowerEffect)?.PowerLevel ?? 0
                                  orderby _powerLevel descending
                                  select new
                                  {
                                      Light = _light,
                                      Level = _powerLevel
                                  }).FirstOrDefault();

                if (_brightest != null)
                {
                    // compare light and dark
                    if (_brightest.Level < _darkest.Level)
                    {
                        // more dark than light
                        self.SetLightLevel(LightRange.NearShadow);
                        self.InMagicalDarkness = true;
                        self.BestIlluminator = null;
                        return;
                    }
                    else
                    {
                        // but do not use magical lights to track illumination
                        self.InMagicalDarkness = false;
                    }
                }
                else
                {
                    // only found darkness
                    self.SetLightLevel(LightRange.NearShadow);
                    self.InMagicalDarkness = true;
                    self.BestIlluminator = null;
                    return;
                }
            }
            else
            {
                // not in magical darkness, so time to check the normal illuminations
                self.InMagicalDarkness = false;
            }

            #endregion

            // Normal illumination mechanisms (if a DarknessShrouded effect is active, do not use magical light sources)
            // ... basically, darkness may be overridden by higher magickal light, but then that light cannot be used to illuminate
            var _level = LightRange.OutOfRange;
            IIllumination _bestLight = null;
            var _allIlluminators = self.GetIlluminators().Distinct().ToList();

            // maximum light available from illuminators (so we don't keep looking if we don't need to)
            if (_allIlluminators.Any())
            {
                var _maxLight = _allIlluminators.Max(_ill => _ill.MaximumLight);
                foreach (var _obj in self.IlluminableObjects)
                {
                    var _processed = new List<IInteract>();
                    foreach (var _illuminator in _allIlluminators)
                    {
                        // must be active
                        if (!_processed.Contains(_illuminator.LightHandler))
                        {
                            // get best from this illuminator
                            if (_illuminator.IsActive)
                            {
                                // if target in magical darkness, do not use magical light (was consumed to cancel the darkness)
                                if ((_darkest == null) || !(_illuminator.Source is MagicPowerEffect))
                                {
                                    var _lightUp = new Illuminate(null, self, _illuminator);
                                    var _lightInteract = new Interaction(null, _illuminator.LightHandler, _obj, _lightUp);
                                    _illuminator.LightHandler.HandleInteraction(_lightInteract);
                                    if (_lightInteract.Feedback.Count != 0)
                                    {
                                        var _result = _lightInteract.Feedback
                                            .OfType<IlluminateResult>()
                                            .FirstOrDefault();
                                        if (_result != null)
                                        {
                                            if (_result.Level > _level)
                                            {
                                                _level = _result.Level;
                                                _bestLight = _result.LightSource;
                                            }
                                        }
                                    }
                                }
                            }

                            // handling picks the best light source anyway, so no need to run through multiple ones on a target
                            _processed.Add(_illuminator.LightHandler);
                        }
                        if (_level == _maxLight)
                            break;
                    }
                    if (_level == _maxLight)
                        break;
                }
            }

            // set light level
            self.SetLightLevel(_level);
            self.BestIlluminator = _bestLight;
        }
        #endregion
    }
}
