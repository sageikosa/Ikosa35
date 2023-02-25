using System;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class RequireSaveFeedback : InteractionFeedback
    {
        public RequireSaveFeedback(object source, SaveMode saveMode)
            : base(source)
        {
            _Mode = saveMode;
        }

        private SaveMode _Mode;
        public SaveMode SaveMode { get { return _Mode; } }
    }
}
