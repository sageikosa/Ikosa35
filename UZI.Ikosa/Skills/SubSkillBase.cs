using Newtonsoft.Json;
using System;

namespace Uzi.Ikosa.Skills
{
    [Serializable]
    public abstract class SubSkillBase
    {
        [NonSerialized, JsonIgnore]
        private SkillInfoAttribute _Info = null;
        public virtual string FocusName
            => (_Info ??= SkillBase.SkillInfo(GetType())).Name;
    }
}
