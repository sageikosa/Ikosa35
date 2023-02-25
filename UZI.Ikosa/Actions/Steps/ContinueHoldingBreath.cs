using System;
using Uzi.Core;
using Uzi.Ikosa.Creatures;

namespace Uzi.Ikosa.Actions.Steps
{
    /// <summary>Attempt to continue holding breath after normal period exceeded</summary>
    [Serializable]
    public class ContinueHoldingBreath : PreReqListStepBase
    {
        #region construction
        /// <summary>Attempt to continue holding breath after normal period exceeded</summary>
        public ContinueHoldingBreath(Creature critter, HoldingBreath holdingBreath)
            : base((CoreProcess)null)
        {
            _Critter = critter;
            _Holding = holdingBreath;
            _Check = new SuccessCheckPrerequisite(_Holding, new Qualifier(_Critter, _Holding, _Critter),
                @"Check.HoldBreath", @"Continue to Hold Breath",
                new SuccessCheck(_Critter.Abilities.Constitution, _Holding.BreathlessCounter.ContinueDifficulty, _Holding), false);
            _PendingPreRequisites.Enqueue(_Check);
        }
        #endregion

        #region private data
        private SuccessCheckPrerequisite _Check;
        private Creature _Critter;
        private HoldingBreath _Holding;
        #endregion

        protected override bool OnDoStep()
        {
            if (!_Check.Success)
            {
                _Critter.AddAdjunct(new Drowning(_Holding));
            }
            return true;
        }
    }
}
