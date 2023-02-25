using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Services;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class AdvancementStep : PreReqListStepBase, IStoppableStep
    {
        public AdvancementStep(CoreSettingContextSet ctxSet)
            : base((CoreProcess)null)
        {
            _PendingPreRequisites.Enqueue(new AdvancementValid(ctxSet));
        }

        public void StopStep()
        {
            var _provider = MasterServices.CreatureProvider;
            foreach (var _critter in (from _login in MasterServices.MapContext.CreatureLoginsInfos
                                      let _crtr = _provider.GetCreature(_login.ID)
                                      where (_crtr?.HasActiveAdjunct<AdvancementCapacity>() ?? false)
                                      select _crtr).ToList())
            {
                // rip back until we do not find unlocked log-items
                while (_critter.AdvancementLog.FirstUnlockedAdvancementLogItem() != null)
                    _critter.AdvancementLog.RemoveLast();
            }
            DispensedPrerequisites.OfType<AdvancementValid>().FirstOrDefault()?
                .MergeFrom(new WaitReleasePrerequisiteInfo());
        }

        protected override bool OnDoStep()
        {
            return true;
        }
    }
}
