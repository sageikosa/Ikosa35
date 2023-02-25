using System;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Must Flee</summary>
    [Serializable]
    public class RepulsedEffect : PredispositionBase
    {
        public RepulsedEffect(MagicPowerEffect source, int checkValue)
            : base(source)
        {
            _Check = checkValue;
        }

        private int _Check;
        public int CheckValue { get { return _Check; } }

        public MagicPowerEffect MagicPowerEffect { get { return Source as MagicPowerEffect; } }

        public override string Description { get { return @"Repulsed (must flee or cower)"; } }
        public override object Clone() { return new RepulsedEffect(MagicPowerEffect, CheckValue); }
    }
}
