using Uzi.Core.Contracts;
using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Actions.Steps
{
    /// <summary>
    /// Reactive Step to be added to a process that may require concentration (typically spells, or skill uses that provoke).
    /// </summary>
    [Serializable]
    public class DistractionStep : CoreStep
    {
        // TODO: motion distraction
        // TODO: entangled distraction
        // TODO: normal weather distraction
        // TODO: grappling/pinned distraction

        #region construction
        public DistractionStep(CoreActivity activity, Interaction distraction, int difficulty)
            : base(activity)
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
        public CoreActivity Activity => Process as CoreActivity; 

        #region public override StepPrerequisite NextPrerequisite()
        /// <summary>Returns the success check prerequisite needed to overcome the distraction</summary>
        protected override StepPrerequisite OnNextPrerequisite()
        {
            if (IsDispensingPrerequisites)
            {
                var _critter = Activity.Actor as Creature;
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
            if (FailingPrerequisite != null)
            {
                (Activity.Action as IInterruptable)?.Interrupted();
                AppendFollowing(Activity.GetActivityResultNotifyStep(@"Distracted"));
                AppendFollowing(new TerminationStep(Activity, null));
            }
            else
            {
                AppendFollowing(Activity.GetActivityResultNotifyStep(@"Not Distracted"));
            }
            return true;
        }
        #endregion
    }
}
