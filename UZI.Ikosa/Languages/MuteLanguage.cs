using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Languages
{
    [Serializable]
    public class MuteLanguage : Language
    {
        public MuteLanguage(Language language)
            : base(language.Source)
        {
            _Base = language;
        }

        #region data
        private Language _Base;
        #endregion

        public Language Base => _Base;

        public override string Name => $@"{_Base.Name} (Mute)";
        public override bool CanProject => false;
        public override string Alphabet => _Base.Alphabet;
        public override Language GetCopy(object source) => new MuteLanguage(Base.GetCopy(source));
    }
}
