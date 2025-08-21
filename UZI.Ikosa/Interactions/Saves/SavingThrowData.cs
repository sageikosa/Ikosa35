using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>General save (action will determine effect)</summary>
    public class SavingThrowData : InteractData
    {
        public SavingThrowData(Creature critter, SaveMode saveMode, Deltable saveRoll)
            : base(critter)
        {
            SaveMode = saveMode;
            SaveRoll = saveRoll;
        }

        public SaveMode SaveMode { get; set; }

        public Deltable SaveRoll { get; private set; }
        public bool Success(Qualifier qualifier)
        {
            var _soft = (qualifier?.Target is IProvideSaves _prov)
                ? _prov.GetBestSoftSave(this)
                : null;
            try
            {
                // add in for this check
                if (_soft != null)
                {
                    SaveRoll.Deltas.Add(_soft);
                }

                var _check = Deltable.GetCheckNotify(qualifier?.Target?.ID, $@"{SaveMode.SaveType} Save", null, @"Difficulty");
                _check.OpposedInfo = SaveMode.Difficulty;
                return (SaveRoll.BaseValue == 20)
                    || ((SaveRoll.QualifiedValue(qualifier, _check.CheckInfo) >= SaveMode.Difficulty.Result) && (SaveRoll.BaseValue != 1));
            }
            finally
            {
                // remove when done
                if (_soft != null)
                {
                    SaveRoll.Deltas.Remove(_soft);
                }
            }
        }
    }
}
