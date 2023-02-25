using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class EnableMechanismStep : CoreStep
    {
        public EnableMechanismStep(CoreActivity activity, IDisableable disableable)
            : base(activity)
        {
            _Disableable = disableable;
        }

        #region data
        private IDisableable _Disableable;
        #endregion

        public IDisableable Disableable => _Disableable;
        public CoreActivity Activity => Process as CoreActivity;

        public override string Name => @"Enable Mechanism";
        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
        {
            if (Activity?.Actor is Creature _critter)
            {
                if (ManipulateTouch.CanManipulateTouch(_critter, Disableable))
                {
                    var _check = Deltable.GetCheckNotify(Disableable.ID, @"Enable Difficulty", _critter.ID, @"Disable Skill");
                    var _iAct = new Interaction(_critter, Disableable, _critter, null);
                    var _difficulty = Disableable.DisableDifficulty.QualifiedValue(_iAct, _check.CheckInfo);

                    // extra difficult if confused about previous disablement
                    if (Disableable.ConfusedDisablers.Contains(_critter.ID))
                    {
                        _check.CheckInfo.AddDelta(@"Confused", 2);
                        _check.CheckInfo.Result += 2;
                        _difficulty += 2;
                    }

                    // TODO: take 10?
                    var _score = new ScoreDeltable(DiceRoller.RollDie(_critter.ID, 20, @"Enable", Name),
                        _critter.Skills.Skill<DisableMechanismSkill>(), @"Disable Skill");

                    var _result = _score.Score.QualifiedValue(_iAct, _check.OpposedInfo);
                    var _difference = _difficulty - _result;

                    // notify of completion
                    if (_difference <= 0)
                    {
                        // success: remove any disablements, clear confusion list
                        Disableable.Adjuncts.OfType<DisabledObject>().FirstOrDefault().Eject();
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
                        // aware of failure
                        Activity.SendActivityResult(@"Failed");
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
