using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class EscapeStep : PreReqListStepBase
    {
        public EscapeStep(Creature critter, EscapeArtistry escapeArtistry) 
            : base((CoreProcess)null)
        {
            _Critter = critter;
            _Escape = escapeArtistry;
            _Check = new SuccessCheckPrerequisite(_Escape, new Qualifier(_Critter, _Escape, EscapeSource.EscapeFrom),
                @"Check.Escape", @"Use Escape Artistry",
                new SuccessCheck(_Critter.Skills.Skill<EscapeArtistSkill>(), EscapeSource.EscapeDifficulty.EffectiveValue, _Escape), true);
        }

        #region private data
        private SuccessCheckPrerequisite _Check;
        private Creature _Critter;
        private EscapeArtistry _Escape;
        #endregion

        public ICanEscape EscapeSource => _Escape.EscapeSource;

        protected override bool OnDoStep()
        {
            if (_Check.Success)
            {
                EscapeSource.DoEscape();
            }
            return true;
        }
    }
}
