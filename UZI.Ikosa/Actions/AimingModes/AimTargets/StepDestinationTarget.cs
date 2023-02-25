using System;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public abstract class StepDestinationTarget : AimTarget
    {
        protected StepDestinationTarget(string key) : base(key, null)
        {
        }

        /// <summary>Reports how many steps are in the destination</summary>
        public abstract int StepCount { get; }

        /// <summary>Gets the crossing faces for a particular step in the sequence</summary>
        public abstract AnchorFace[] CrossingFaces(AnchorFace gravity, int stepIndex);

        public abstract int GetHeading(AnchorFace gravity, int stepIndex);

        public abstract UpDownAdjustment GetUpDownAdjustment(AnchorFace gravity, int stepIndex);
    }
}
