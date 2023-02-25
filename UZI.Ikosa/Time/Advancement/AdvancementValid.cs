using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class AdvancementValid : WaitReleasePrerequisite
    {
        public AdvancementValid(CoreSettingContextSet ctxSet)
            : base(ctxSet)
        {
        }

        private CoreSettingContextSet ContextSet => Source as CoreSettingContextSet;

        public override bool IsReady
            => base.IsReady 
            || !(ContextSet?.GetCoreIndex()
                .Select(_idx => _idx.Value).OfType<Creature>()
                .Any(_c => _c.HasActiveAdjunct<AdvancementCapacity>()) ?? false);
    }
}
