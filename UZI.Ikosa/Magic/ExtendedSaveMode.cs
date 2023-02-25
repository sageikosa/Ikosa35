using System;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class ExtendedSaveMode : SaveMode
    {
        public ExtendedSaveMode(SaveType saveType, SaveEffect saveEffect, DeltaCalcInfo difficulty, string description) :
            base(saveType, saveEffect, difficulty)
        {
            Description = description;
        }

        public string Description { get; private set; }
    }
}
