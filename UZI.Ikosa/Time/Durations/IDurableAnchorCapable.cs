using Uzi.Ikosa.Adjuncts;
using Uzi.Core;

namespace Uzi.Ikosa.Time
{
    /// <summary>
    /// Represents a durable effect that needs to set stuff up and tear stuff down only once per association
    /// </summary>
    public interface IDurableAnchorCapable : ICapability
    {
        object OnAnchor(IAdjunctTracker source, IAdjunctable target);
        void OnEndAnchor(IAdjunctTracker source, IAdjunctable target);
    }
}
