using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Awareness level of source.  If difficulty check succeeds, use the spotSuccess instead.
    /// </summary>
    public class ObserveSpotFeedback : ObserveFeedback
    {
        public ObserveSpotFeedback(object source, int difficulty, IEnumerable<KeyValuePair<Guid, AwarenessLevel>> spotSuccesses, 
            IEnumerable<KeyValuePair<Guid, AwarenessLevel>> spotFails)
            : base(source, spotFails)
        {
            Difficulty = difficulty;
            SpotSuccesses = spotSuccesses;
        }

        // TODO: consider making this a Deltable
        public readonly int Difficulty;
        public readonly IEnumerable<KeyValuePair<Guid, AwarenessLevel>> SpotSuccesses;
    }
}
