using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Standard Visual Effect Handler</summary>
    [Serializable]
    public class VisualEffectHandler : IInteractHandler
    {
        #region public static VisualEffect GetSightedVisualEffect(List<SensoryBase> sight, LightRange level, double distance)
        public static VisualEffect GetSightedVisualEffect(List<SensoryBase> sight, LightRange level, double distance)
        {
            var _effect = VisualEffect.Unseen;
            if (sight.Any())
            {
                if (sight.Any(_ir => _ir.IgnoresVisualEffects))
                {
                    // sighted sense ignoring visual effects doesn't care about light levels
                    _effect = VisualEffect.Normal;
                }
                else if (level >= LightRange.Bright)
                {
                    // brightly lit
                    if (sight.Any(_ir => _ir.UsesLight))
                    {
                        // very or just plain bright
                        _effect = (level >= LightRange.VeryBright)
                            ? VisualEffect.Brighter
                            : VisualEffect.Normal;
                    }
                    else
                    {
                        // dark vision (and true seeing...)
                        _effect = VisualEffect.Monochrome;
                    }
                }
                else if (level >= LightRange.NearShadow)
                {
                    // regular shadowy
                    if (sight.Any(_ir => _ir.UsesLight && _ir.LowLight))
                    {
                        _effect = (level == LightRange.NearBoost)
                            ? VisualEffect.Brighter     // low light at threshold of near boost
                            : VisualEffect.Normal;      // low light
                    }
                    else if (sight.Any(_ir => !_ir.UsesLight))
                    {
                        _effect = sight.Any(_ir => _ir.UsesLight)
                            ? VisualEffect.Normal       // darkvision and sensor also uses normal vision
                            : VisualEffect.Monochrome;  // darkvision
                    }
                    else
                    {
                        _effect = (level == LightRange.NearBoost)
                            ? VisualEffect.DimTo75      // near boost
                            : VisualEffect.DimTo50;     // near shadow
                    }
                }
                else if (sight.Any(_ir => !_ir.UsesLight))
                {
                    _effect = VisualEffect.Monochrome;
                }
                else if (level >= LightRange.FarShadow)
                {
                    if (sight.Any(_ir => _ir.UsesLight && _ir.LowLight))
                    {
                        // low light vision is all that's left if we haven't found darkvision
                        _effect = (level == LightRange.FarBoost)
                            ? VisualEffect.DimTo75
                            : VisualEffect.DimTo50;
                    }
                    else
                    {
                        // far shade but on fringe for near shade
                        if (level == LightRange.FarBoost)
                        {
                            _effect = VisualEffect.DimTo25;
                        }
                    }
                }
                else
                {
                    // beyond far shade
                    if ((level == LightRange.ExtentBoost)
                        && sight.Any(_ir => _ir.UsesLight && _ir.LowLight))
                    {
                        _effect = VisualEffect.DimTo25;
                    }
                }
            }
            return _effect;
        }
        #endregion

        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is VisualEffectData _visEffect)
            {
                var _senses = _visEffect.Senses;

                // ensure the set is filtered
                var _feedback = new VisualEffectFeedback(this);
                workSet.Feedback.Add(_feedback);

                if (_senses.Count == 0)
                {
                    // if no active senses perform terrain visualization, no point in generating models
                    _feedback.VisualEffects.Add(typeof(SenseEffectExtension), VisualEffect.Skip);
                    return;
                }

                // start at no visualization
                var _effect = VisualEffect.Unseen;

                if (!_senses.Any(_s => _s.UsesSight))
                {
                    // form-only
                    _effect = VisualEffect.FormOnly;
                }
                else
                {
                    var _distance = _visEffect.SourceRegion.NearDistance(_visEffect.TargetLocator.GeometricRegion);

                    #region magic dark
                    // any magic darks on objects in this locator
                    if ((from _obj in _visEffect.TargetLocator.ICoreAs<ICoreObject>()
                         from _a in _obj.Adjuncts.OfType<DarknessShrouded>()
                         where _a.IsActive
                         select _a).Any())
                    {
                        // dim under magic dark
                        _effect = VisualEffect.DimTo50;

                        foreach (var _magicDarkPiercing in _senses.Where(_s => _s.IgnoresVisualEffects
                            && (_distance <= _s.Range)))
                        {
                            if (!_magicDarkPiercing.UsesSenseTransit
                                || _magicDarkPiercing.CarrySenseInteraction(
                                    _visEffect.TargetLocator.Map,
                                    _visEffect.SourceRegion,
                                    _visEffect.TargetLocator.GeometricRegion,
                                    ITacticalInquiryHelper.EmptyArray
                                ))
                            {
                                if (_magicDarkPiercing.UsesSight)
                                {
                                    // obtained highest detail
                                    _effect = VisualEffect.Normal;
                                    break;
                                }
                                else
                                {
                                    _effect = VisualEffect.FormOnly;
                                }
                            }
                        }
                    }
                    #endregion

                    // still need to determine visual effect
                    if (_effect == VisualEffect.Unseen)
                    {
                        // in range visual senses
                        var _sight = _senses.Where(_s => _s.UsesSight && _s.Range >= _distance).ToList();
                        _effect = GetSightedVisualEffect(_sight, _visEffect.TargetLocator.LightLevel, _distance);

                        // if not normal or monochrome, allow non-sighted sense a chance to affect visualization
                        if ((_effect != VisualEffect.Normal)
                            && (_effect != VisualEffect.Brighter)
                            && (_effect != VisualEffect.Monochrome))
                        {
                            foreach (var _formSense in _senses.Where(_s => !_s.UsesSight
                                && (_distance <= _s.Range)))
                            {
                                if (!_formSense.UsesSenseTransit
                                    || _formSense.CarrySenseInteraction(
                                        _visEffect.TargetLocator.Map,
                                        _visEffect.SourceRegion,
                                        _visEffect.TargetLocator.GeometricRegion,
                                        ITacticalInquiryHelper.EmptyArray))
                                {
                                    _effect = VisualEffect.FormOnly;
                                }
                            }
                        }
                    }
                }

                // ensures any required manipulation of inner model is done on a distinct instance
                _feedback.VisualEffects.Add(typeof(SenseEffectExtension), _effect);
            }
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(VisualEffectData);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
        #endregion
    }
}