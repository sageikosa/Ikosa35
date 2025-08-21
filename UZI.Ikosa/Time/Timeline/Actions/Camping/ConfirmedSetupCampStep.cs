using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class ConfirmedSetupCampStep : CoreStep
    {
        public ConfirmedSetupCampStep(CoreProcess process, DecideCampGroup decision)
            : base(process)
        {
            _Decision = decision;
        }

        #region data
        private DecideCampGroup _Decision;
        #endregion

        public DecideCampGroup DecideCampGroup => _Decision;

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        public override bool IsDispensingPrerequisites => false;

        protected override bool OnDoStep()
        {
            // get site chosen
            var _campFocus = DecideCampGroup.DecideCampSite.Anchor as CampSiteFocus;

            // went from deciding to actually doing
            var _campingGroup = new CampingGroup(typeof(ConfirmedSetupCampStep));
            var _site = new CampSite(_campingGroup);
            _campFocus.AddAdjunct(_site);

            // everybody in the social group will start camping
            var _team = DecideCampGroup.TeamGroup;
            foreach (var _member in _team.TeamCreatures)
            {
                // consume any remaining budgets for camp members
                var _budget = _member.GetLocalActionBudget();
                if (_budget == null)
                {
                    // needs a budget to consume
                    var _tracker = (Process.ProcessManager as IkosaProcessManager)?.LocalTurnTracker;
                    _budget = _member.CreateActionBudget(_tracker.RoundMarker) as LocalActionBudget;
                    _tracker.AddBudget(_budget);
                }
                _budget.ConsumeBudget(Contracts.TimeType.Total);

                // schedule next activity for all budgets on camp members
                _budget.NextActivity =
                    new CoreActivity(
                        _budget.Actor,
                        new SetupCamp(_team, new ActionTime(Minute.UnitFactor), @"200"),
                        [
                            new ValueTarget<CampingGroup>(nameof(CampingGroup), _campingGroup)
                        ]);
            }

            // eject decide setup camp
            DecideCampGroup.DecideCampSite.Eject();
            return true;
        }
    }
}
