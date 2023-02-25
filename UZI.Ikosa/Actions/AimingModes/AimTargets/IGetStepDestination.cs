using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    public interface IGetStepDestination
    {
        /// <summary>Reports how many steps are in the destination</summary>
        int StepCount { get; }

        /// <summary>Gets the crossing faces for a particular step in the sequence</summary>
        AnchorFace[] CrossingFaces(AnchorFace gravity, int stepIndex);
    }
}
