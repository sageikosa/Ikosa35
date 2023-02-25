using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Time
{
    /// <summary>Allows definition of effect duration rules</summary>
    [Serializable]
    public class DurationRule
    {
        #region Construction
        public DurationRule(DurationType timeType)
        {
            _DType = timeType;
            _Parts = null;
        }

        public DurationRule(IEnumerable<SpanRulePart> spanParts)
        {
            _DType = DurationType.Span;
            _Parts = spanParts;
        }

        public DurationRule(DurationType timeType, IEnumerable<SpanRulePart> spanParts)
        {
            _DType = timeType;
            _Parts = spanParts;
        }

        public DurationRule(DurationType timeType, SpanRulePart spanRule)
        {
            _DType = timeType;
            _Parts = new SpanRulePart[] { spanRule };
        }

        public DurationRule(string description)
        {
            _Description = description;
            _DType = DurationType.Custom;
            _Parts = null;
        }
        #endregion

        #region data
        private DurationType _DType;
        private IEnumerable<SpanRulePart> _Parts;
        private bool _RoundUp = false;
        private int _MaxLevel = int.MaxValue;
        private string _Description = null;
        #endregion

        public string Description
            => _Description
            ?? DurationType switch
            {
                DurationType.ConcentrationPlusSpan => @"Concentration + ",  // TODO: calc from span rule parts
                DurationType.Span => @"Span",                               // TODO: calc from span rule parts
                _ => DurationType.ToString()
            };

        public DurationType DurationType => _DType;
        public IEnumerable<SpanRulePart> SpanParts => _Parts;
        public bool RoundUp { get => _RoundUp; set => _RoundUp = value; }
        public int MaxLevel { get => _MaxLevel < 1 ? int.MaxValue : _MaxLevel; set => _MaxLevel = value; }

        #region public double EndTime(PowerSource powerSource, AimTarget aimedAt)
        /// <summary>
        /// Effective end time of duration based on current campaign time, 
        /// and accounting for the need to establish a time setting at the end of the nearest round
        /// </summary>
        public double EndTime(CoreActor actor, IPowerActionSource powerSource, IInteract target)
        {
            var _map = actor.GetLocated().Locator.Map;
            if (RoundUp)
                return Math.Ceiling(EffectiveSpan(powerSource, target).SpanLength + _map.CurrentTime);
            else
                return EffectiveSpan(powerSource, target).SpanLength + _map.CurrentTime;
        }
        #endregion

        #region public Duration EffectiveSpan(PowerSource powerSource, AimTarget aimedAt)
        public Duration EffectiveSpan(IPowerActionSource powerSource, IInteract target)
        {
            // if power class is direct get creature for class
            Creature _critter = null;
            if (powerSource.PowerClass is AdvancementClass _trueClass)
            {
                _critter = _trueClass.Creature;
            }

            return EffectiveSpan(powerSource.PowerClass.ClassPowerLevel.QualifiedValue(
                new Qualifier(_critter, powerSource, target)));
        }
        #endregion+

        #region public Duration EffectiveSpan(int powerLevel)
        public Duration EffectiveSpan(int powerLevel)
        {
            double _span = 0;
            switch (DurationType)
            {
                case DurationType.Instantaneous:
                    break;

                case DurationType.Concentration:
                    {
                        if (SpanParts?.Any() ?? false)
                        {
                            var _powerLevel = Math.Min(powerLevel, MaxLevel);
                            foreach (var _srp in SpanParts)
                            {
                                _span += _srp.EffectiveSpan(_powerLevel).SpanLength;
                            }
                        }
                        else
                        {
                            _span = Year.UnitFactor * 1e14; // One hundred trillion years in the future (Short scale)
                        }
                    }
                    break;

                case DurationType.Permanent:
                    _span = Year.UnitFactor * 1e14; // One hundred trillion years in the future (Short scale)
                    break;

                case DurationType.Custom:
                case DurationType.Span:
                default:
                    {
                        var _powerLevel = Math.Min(powerLevel, MaxLevel);
                        foreach (var _srp in SpanParts)
                        {
                            _span += _srp.EffectiveSpan(_powerLevel).SpanLength;
                        }
                    }
                    break;
            }
            return new Duration(_span);
        }
        #endregion

        public double Resolution
            => SpanParts.Min(_sp => _sp.TimeUnit.BaseUnitFactor);
    }
}