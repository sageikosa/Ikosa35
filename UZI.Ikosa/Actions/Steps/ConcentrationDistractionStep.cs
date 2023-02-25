using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class ConcentrationDistractionStep : CoreStep
    {
        #region construction
        public ConcentrationDistractionStep(CoreProcess process, Interaction distraction, int difficulty, IDistractable distractable)
            : base(process)
        {
            _Difficulty = difficulty;
            _Distraction = distraction;
            _Distractable = distractable;
        }
        #endregion

        #region state
        private int _Difficulty;
        private Interaction _Distraction;
        private IDistractable _Distractable;
        #endregion

        public int Difficulty => _Difficulty;
        public Interaction Distraction => _Distraction;
        public IDistractable Distractable => _Distractable;

        #region public override StepPrerequisite NextPrerequisite()
        protected override StepPrerequisite OnNextPrerequisite()
        {
            if (IsDispensingPrerequisites)
            {
                var _critter = Distraction.Target as Creature;
                return new SuccessCheckPrerequisite(this, Distraction, @"Distraction", @"Distraction",
                    new SuccessCheck(_critter.Skills.Skill<ConcentrationSkill>(),
                    Distractable.ConcentrationBase.EffectiveValue + _Difficulty, Distraction?.Source), true);
            }
            return null;
        }
        #endregion

        public override bool IsDispensingPrerequisites
            => DispensedPrerequisites.Count() == 0;

        #region protected override bool OnDoStep()
        /// <summary>If the success check failed, the activity is terminated</summary>
        protected override bool OnDoStep()
        {
            var _critter = Distraction.Target as Creature;
            if (FailingPrerequisite != null)
            {
                Distractable.Interrupted();
                EnqueueNotify(new BadNewsNotify(_critter.ID, @"Distraction to Concentration", new Info { Message = @"Distracted" }), _critter.ID);
            }
            else
            {
                EnqueueNotify(new GoodNewsNotify(_critter.ID, @"Distraction", new Info { Message = @"Not Distracted" }), _critter.ID);
            }
            return true;
        }
        #endregion
    }
}
