using System;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>
    /// Linear formula range based on caster level
    /// </summary>
    [Serializable]
    public class LinearRange : Range
    {
        #region Construction
        public LinearRange(double offSet, double perStep, int stepSize)
        {
            _Offset = offSet;
            _PerStep = perStep;
            _StepSize = stepSize;
            _GroundStep = 0;
            _MaxRange = double.MaxValue;
        }

        public LinearRange(double offSet, double perStep, int stepSize, int groundStep)
        {
            _Offset = offSet;
            _PerStep = perStep;
            _StepSize = stepSize;
            _GroundStep = groundStep;
            _MaxRange = double.MaxValue;
        }

        public LinearRange(double offSet, double perStep, int stepSize, int groundStep, double maxRange)
        {
            _Offset = offSet;
            _PerStep = perStep;
            _StepSize = stepSize;
            _GroundStep = groundStep;
            _MaxRange = maxRange;
        }
        #endregion

        #region Private Data
        private double _Offset;
        private double _PerStep;
        private int _StepSize;
        private int _GroundStep;
        private double _MaxRange;
        #endregion

        /// <summary>Fixed amount</summary>
        public double OffSet { get { return _Offset; } }

        /// <summary>How much to add per step</summary>
        public double PerStep { get { return _PerStep; } }

        /// <summary>How many power levels count as a step</summary>
        public int StepSize { get { return _StepSize; } }

        /// <summary>Base power level from which steps start</summary>
        public int GroundStep { get { return _GroundStep; } }

        /// <summary>Absolute upper limit to effective range</summary>
        public double MaxRange { get { return _MaxRange; } }

        public override double EffectiveRange(CoreActor actor, int powerLevel)
        {
            double _range = OffSet + PerStep * Math.Floor(Convert.ToDouble((powerLevel - GroundStep) / StepSize));
            return Math.Min(MaxRange, _range);
        }

        public override RangeInfo ToRangeInfo(CoreAction action, CoreActor actor)
        {
            return ToInfo<RangeInfo>(action, actor);
        }
    }
}
