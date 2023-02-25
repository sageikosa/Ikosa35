using System;

namespace Uzi.Ikosa.Time
{
    /// <summary>
    /// Allows for systematic definitions of fixed and level variable durations
    /// </summary>
    [Serializable]
    public class SpanRulePart
    {
        public SpanRulePart(double numberUnits, TimeUnit unitType)
        {
            _Units = numberUnits;
            _Time = unitType;
            _Factor = 0;
        }

        public SpanRulePart(double numberUnits, TimeUnit unitType, int levelFactor)
        {
            _Units = numberUnits;
            _Time = unitType;
            _Factor = levelFactor;
        }

        #region Private Data
        public double _Units;
        public TimeUnit _Time;
        public int _Factor;
        #endregion

        public double NumberUnits { get { return _Units; } }
        public TimeUnit TimeUnit { get { return _Time; } }
        public int LevelFactor { get { return _Factor; } } 

        public Duration EffectiveSpan(int powerLevel)
        {
            if (LevelFactor > 0)
            {
                int _levelFactor = powerLevel / LevelFactor;
                return new Duration(TimeUnit.BaseUnitFactor * NumberUnits * _levelFactor);
            }
            else
            {
                return new Duration(TimeUnit.BaseUnitFactor * NumberUnits);
            }
        }
    }
}
