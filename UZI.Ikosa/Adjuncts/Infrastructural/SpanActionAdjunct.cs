using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Indicates that the creature (anchor) is currently engaged in a span action.  
    /// Also, provides a mechanism to signal the turn tracker that the action has been interrupted.
    /// </summary>
    [Serializable]
    public class SpanActionAdjunct : Adjunct
    {
        public SpanActionAdjunct(CoreAction action)
            : base(action)
        {
            _Interrupted = false;
        }

        #region state
        private bool _Interrupted;
        #endregion

        public bool IsInterrupted { get => _Interrupted;  set => _Interrupted = value; } 

        public CoreAction Action => Source as CoreAction; 

        public override object Clone()
            =>new SpanActionAdjunct(Action);
    }
}
