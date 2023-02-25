using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class BurstFreeStep : PreReqListStepBase
    {
        public BurstFreeStep(CoreActivity activity, Creature critter, BurstFree burstFree) 
            : base(activity)
        {
            _Critter = critter;
            _Burst = burstFree;
            _Check = new SuccessCheckPrerequisite(_Burst, new Qualifier(_Critter, _Burst, BurstFreeSource.BurstFrom),
                @"Check.Escape", @"Use Escape Artistry",
                new SuccessCheck(_Critter.Abilities.Strength, BurstFreeSource.BurstFreeDifficulty.EffectiveValue, _Burst), true);
        }

        #region private data
        private SuccessCheckPrerequisite _Check;
        private Creature _Critter;
        private BurstFree _Burst;
        #endregion

        public ICanBurstFree BurstFreeSource => _Burst.BurstFreeSource;

        protected override bool OnDoStep()
        {
            if (_Check.Success)
            {
                BurstFreeSource.DoBurstFree();
            }
            return true;
        }
    }
}
