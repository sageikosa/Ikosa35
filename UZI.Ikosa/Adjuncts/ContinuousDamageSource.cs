using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class ContinuousDamageSource : Adjunct, ITrackTime
    {
        public ContinuousDamageSource(object source, List<DamageRule> damages,
            double nextTime, double endTime, double resolution, int powerLevel)
            : base(source)
        {
            _Damages = damages;
            _NextTime = nextTime;
            _EndTime = endTime;
            _Resolution = resolution;
            _PowerLevel = powerLevel;
        }

        #region data
        private List<DamageRule> _Damages;
        private double _NextTime;
        private double _EndTime;
        private double _Resolution;
        private int _PowerLevel;
        #endregion

        public List<DamageRule> DamageRules => _Damages.ToList();
        public double NextTime => _NextTime;
        public int PowerLevel => _PowerLevel;

        public override object Clone()
            => new ContinuousDamageSource(Source, DamageRules, NextTime, EndTime, Resolution, PowerLevel);

        /// <summary>allow partial removal of continuous damages</summary>
        public void RemoveDamageRule(DamageRule rule)
        {
            if (_Damages.Contains(rule))
            {
                _Damages.Remove(rule);

                // empty == done
                if (!_Damages.Any())
                {
                    Eject();
                }
            }
        }

        #region ITrackTime Members
        public double EndTime => _EndTime;

        public double Resolution => _Resolution;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (direction == TimeValTransition.Entering)
            {
                if (timeVal >= NextTime)
                {
                    // start a continuous damage process
                    Anchor?.StartNewProcess(new ContinuousDamageStep(this), @"Continuous");

                    // prepare for next time
                    _NextTime += Resolution;
                }

                if (timeVal >= EndTime)
                {
                    // done
                    Eject();
                }
            }
        }
        #endregion
    }
}
