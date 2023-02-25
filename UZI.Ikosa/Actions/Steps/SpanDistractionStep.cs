using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Actions.Steps
{
    /// <summary>Step for a process to perform a success check on a span action.</summary>
    [Serializable]
    public class SpanDistractionStep : CoreStep
    {
        #region construction
        public SpanDistractionStep(CoreProcess process, Interaction distraction, int difficulty)
            : base(process)
        {
            _Difficulty = difficulty;
            _Distraction = distraction;
        }
        #endregion

        #region state
        private int _Difficulty;
        private Interaction _Distraction;
        #endregion

        public int Difficulty => _Difficulty;
        public Interaction Distraction => _Distraction;

        #region public override StepPrerequisite NextPrerequisite()
        protected override StepPrerequisite OnNextPrerequisite()
        {
            if (IsDispensingPrerequisites)
            {
                var _critter = Distraction.Target as Creature;
                if (Distraction != null)
                {
                    return new SuccessCheckPrerequisite(this, Distraction, @"Distraction", @"Distraction",
                        new SuccessCheck(_critter.Skills.Skill<ConcentrationSkill>(), _Difficulty, Distraction.Source), true);
                }
                else
                {
                    return new SuccessCheckPrerequisite(this, Distraction, @"Distraction", @"Distraction",
                        new SuccessCheck(_critter.Skills.Skill<ConcentrationSkill>(), _Difficulty, null), true);
                }

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
                var _spanAdj = _critter.Adjuncts.OfType<SpanActionAdjunct>().FirstOrDefault();
                if (_spanAdj != null)
                {
                    _spanAdj.IsInterrupted = true;
                }
                EnqueueNotify(new BadNewsNotify(_critter.ID, @"Distraction to Action", new Info { Message = @"Distracted" }), _critter.ID);
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
