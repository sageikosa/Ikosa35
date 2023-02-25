using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Distracted : PredispositionBase, IMonitorChange<DeltaValue>
    {
        #region construction
        public Distracted(Interaction incident, int difficultyBase)
            : base(incident)
        {
            _Difficulty = new Deltable(difficultyBase);
            _Difficulty.AddChangeMonitor(this);
        }
        #endregion

        #region private data
        private Deltable _Difficulty;
        #endregion

        public Interaction Incident 
            => Source as Interaction; 

        public Deltable BaseDifficulty => _Difficulty; 

        public override string Description
            => @"Distracted";

        public override object Clone()
            => new Distracted(Incident, BaseDifficulty.BaseValue);

        #region IMonitorChange<DeltaValue> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            // once all distractions are gone, we can end the distracted adjunct
            if (BaseDifficulty.EffectiveValue == BaseDifficulty.BaseValue)
                this.Eject();
        }

        #endregion
    }
}
