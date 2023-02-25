using System;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class LoseHealthPointStep : CoreStep
    {
        public LoseHealthPointStep(CoreProcess process, Creature critter)
            : base(process)
        {
            _Critter = critter;
        }

        public Creature _Critter;

        public override string Name { get { return @"Lose 1 Health Point"; } }

        protected override bool OnDoStep()
        {
            if (_Critter != null)
            {
                _Critter.HealthPoints.CurrentValue--;
            }

            // done
            if (_Critter != null)
            {
                EnqueueNotify(new RefreshNotify(true, false, true, false, false), _Critter.ID);
                //this.EnqueueStatus(_Critter.ID,
                //    new Info { Title = Name },
                //    new RefreshNotify(true, false, true, false, false));
            }
            return true;
        }

        protected override StepPrerequisite OnNextPrerequisite() { return null; }
        public override bool IsDispensingPrerequisites { get { return false; } }
    }
}
