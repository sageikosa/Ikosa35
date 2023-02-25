using System;
using Uzi.Ikosa.Movement;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LandMovementProperties : MovementZoneProperties
    {
        public LandMovementProperties(string name, Type source)
            : base(name, typeof(LandMovement))
        {
            _MustCheckToRunCharge = true;
            _Balance = new ConstDelta(0, source);
            _Tumble = new ConstDelta(0, source);
            _SilentStealth = new ConstDelta(0, source);
            _CostFactor = 1;
        }

        #region private data
        // TODO: description...
        private bool _MustCheckToRunCharge;
        private ConstDelta _Balance;
        private ConstDelta _Tumble;
        private ConstDelta _SilentStealth;
        private double _CostFactor;
        #endregion

        public bool MustCheckToRunCharge { get { return _MustCheckToRunCharge; } set { _MustCheckToRunCharge = value; } }

        /// <summary>Extra difficulty for balancing</summary>
        public ConstDelta Balance { get { return _Balance; } set { _Balance = value; } }
        /// <summary>Extra difficulty for tumbling</summary>
        public ConstDelta Tumble { get { return _Tumble; } set { _Tumble = value; } }
        /// <summary>Extra difficulty for silent stealth</summary>
        public ConstDelta SilentStealth { get { return _SilentStealth; } set { _SilentStealth = value; } }

        public string Source { get { return _Balance.Name; } }
        public double CostFactor { get { return _CostFactor; } set { _CostFactor = value; } }

        /// <summary>Intended to be used as a Delta source</summary>
        [Serializable, SourceInfo(@"Topology")]
        public sealed class Topology { }

        /// <summary>Intended to be used as a Delta source</summary>
        [Serializable, SourceInfo(@"Debris")]
        public sealed class Debris { }

        /// <summary>Intended to be used as a Delta source</summary>
        [Serializable, SourceInfo(@"Grip")]
        public sealed class Grip { }
    }
}
