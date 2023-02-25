using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class PersistentNegativeLevel : NegativeLevel, ITrackTime
    {
        public PersistentNegativeLevel(Type riser, double nextRemovalCheck, double cycleTime, DeltaCalcInfo difficulty)
            : base(typeof(PersistentNegativeLevel))
        {
            _NextTime = nextRemovalCheck;
            _CycleTime = cycleTime;
            _Difficulty = difficulty;
            _Riser = riser;
        }

        #region data
        private double _NextTime;
        private double _CycleTime;
        private DeltaCalcInfo _Difficulty;
        #endregion

        public double NextRemovalCheck => _NextTime;
        public double CycleTime => _CycleTime;
        public DeltaCalcInfo Difficulty => _Difficulty;

        public override object Clone()
            => new PersistentNegativeLevel(_Riser, NextRemovalCheck, CycleTime, Difficulty);

        public double Resolution
            => CycleTime >= Day.UnitFactor
            ? Day.UnitFactor
            : Hour.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (timeVal >= NextRemovalCheck)
            {
                _NextTime += CycleTime;
                Anchor?.StartNewProcess(new PersistentNegativeLevelSaveStep(this), @"Negative Level Remove Save");
            }
        }
    }

    [Serializable]
    public class PersistentNegativeLevelSaveStep : PreReqListStepBase
    {
        public PersistentNegativeLevelSaveStep(PersistentNegativeLevel level)
            : base((CoreProcess)null)
        {
            _Level = level;
            _PendingPreRequisites.Enqueue(new SavePrerequisite(NegativeLevel,
                new Qualifier(null, NegativeLevel, NegativeLevel.Anchor as IInteract),
                @"Negative Level", @"Save to remove negative level",
                new SaveMode(SaveType.Fortitude, SaveEffect.Negates, NegativeLevel.Difficulty)));
        }

        #region data
        private PersistentNegativeLevel _Level;
        #endregion

        public PersistentNegativeLevel NegativeLevel => _Level;

        public SavePrerequisite Save
            => GetPrerequisite<SavePrerequisite>();

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            if (Save.Success)
            {
                NegativeLevel.Eject();
            }
            return true;
        }
        #endregion
    }
}
