using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class OpenStatusChangedSoundFeedback : InteractionFeedback
    {
        public OpenStatusChangedSoundFeedback(object source, SoundRef soundRef)
            : base(source)
        {
            _Ref = soundRef;
        }

        #region state
        private SoundRef _Ref;
        #endregion

        public SoundRef SoundRef => _Ref;
    }
}
