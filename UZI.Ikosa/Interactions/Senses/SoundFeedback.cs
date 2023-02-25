using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Interactions
{
    public class SoundFeedback : InteractionFeedback
    {
        public SoundFeedback(object source, DeltaCalcInfo difficultyInfo, DeltaCalcInfo checkInfo)
            : base(source)
        {
            _CheckInfo = checkInfo;
            _DiffInfo = difficultyInfo;
        }

        #region state
        private DeltaCalcInfo _CheckInfo;
        private DeltaCalcInfo _DiffInfo;
        #endregion

        public DeltaCalcInfo CheckInfo => _CheckInfo;
        public DeltaCalcInfo DifficultyInfo => _DiffInfo;
    }
}
