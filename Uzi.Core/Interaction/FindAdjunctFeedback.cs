using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    [Serializable]
    public class FindAdjunctFeedback : InteractionFeedback
    {
        public FindAdjunctFeedback(object source, List<Adjunct> adjuncts)
            : base(source)
        {
            _Adjuncts = adjuncts;
        }

        private List<Adjunct> _Adjuncts;

        public List<Adjunct> Adjuncts { get { return _Adjuncts; } }
    }
}
