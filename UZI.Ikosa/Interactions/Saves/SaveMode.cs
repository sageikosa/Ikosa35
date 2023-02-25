using System;
using System.Collections.Generic;
using System.Windows.Documents;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Describes target save value, and effect of success</summary>
    [Serializable]
    public class SaveMode
    {
        #region ctor()
        public SaveMode(SaveType saveType, SaveEffect saveEffect, DeltaCalcInfo difficulty, bool affectsObject)
        {
            _Type = saveType;
            _Effect = saveEffect;
            _Difficulty = difficulty;
            _Object = affectsObject;
            _Deltas = new List<IQualifyDelta>();
        }

        public SaveMode(SaveType saveType, SaveEffect saveEffect, DeltaCalcInfo difficulty)
        {
            _Type = saveType;
            _Effect = saveEffect;
            _Difficulty = difficulty;
            _Object = SaveType == SaveType.Reflex;
            _Deltas = new List<IQualifyDelta>();
        }
        #endregion

        #region data
        private SaveType _Type;
        private SaveEffect _Effect;
        private DeltaCalcInfo _Difficulty;
        private bool _Object;
        private List<IQualifyDelta> _Deltas;
        #endregion

        public SaveType SaveType => _Type;
        public SaveEffect SaveEffect => _Effect;
        public DeltaCalcInfo Difficulty => _Difficulty;
        public bool Object => _Object;
        public List<IQualifyDelta> QualifiedDeltas => _Deltas;
    }

    [Serializable]
    public enum SaveEffect
    {
        /// <summary>No change to damage or effect</summary>
        None,
        Text,
        /// <summary>Some partial, lesser effect</summary>
        Partial,
        /// <summary>Half damage on a successful save (only applies to damages)</summary>
        Half,
        /// <summary>No damage or effect on a successful save</summary>
        Negates
    }
}
