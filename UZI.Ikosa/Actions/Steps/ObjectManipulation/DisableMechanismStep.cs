using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class DisableMechanismStep : CoreStep
    {
        public DisableMechanismStep(CoreActivity activity, IDisableable disableable)
            : base(activity)
        {
            _Disableable = disableable;
        }

        #region data
        private IDisableable _Disableable;
        #endregion

        public IDisableable Disableable => _Disableable;
        public CoreActivity Activity => Process as CoreActivity;

        public override string Name => @"Disable Mechanism";
        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
        {
            if (Activity?.Actor is Creature _critter)
            {
                if (ManipulateTouch.CanManipulateTouch(_critter, Disableable))
                {
                    var _iAct = new Interaction(_critter, Disableable, _critter, null);
                    var _check = Deltable.GetCheckNotify(Disableable.ID, @"Disable Difficulty", _critter.ID, @"Disable Check");

                    var _difficulty = Disableable.DisableDifficulty.QualifiedValue(_iAct, _check.CheckInfo);
                    // TODO: take 10?
                    var _score = new ScoreDeltable(DiceRoller.RollDie(_critter.ID, 20, @"Disable", Name),
                        _critter.Skills.Skill<DisableMechanismSkill>(), @"Disable Skill");

                    var _result = _score.Score.QualifiedValue(_iAct, _check.OpposedInfo);
                    var _difference = _difficulty - _result;

                    // TODO: bypass trap without disarming (re-activate action?)

                    // if previously failed, only notify of completion, do not attempt additional disablement
                    if (!Disableable.ConfusedDisablers.Contains(_critter.ID))
                    {
                        if (_difference <= 0)
                        {
                            // success
                            if (!Disableable.HasAdjunct<DisabledObject>())
                            {
                                // disable the object
                                Disableable.AddAdjunct(new DisabledObject());
                            }
                            Disableable.ConfusedDisablers.Clear();
                            Activity.SendActivityResult(@"Complete");
                        }
                        else if (_difference >= 5)
                        {
                            // unaware of failure
                            Disableable.FailedDisable(Activity);
                            Activity.SendActivityResult(@"Complete");
                        }
                        else
                        {
                            Activity.SendActivityResult(@"Failed");
                        }
                    }
                    else
                    {
                        Activity.SendActivityResult(@"Complete");
                    }
                }
                else
                {
                    Activity.SendActivityResult(@"Cannot touch");
                }

                // end the activity
                Activity.IsActive = false;
            }
            return true;
        }
    }
}
