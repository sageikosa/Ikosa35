using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class DecideCampMember : GroupMemberAdjunct, ITrackTime, IActionProvider
    {
        public DecideCampMember(DecideCampGroup camping)
            : base(typeof(DecideCampGroup), camping)
        {
        }

        #region data
        private double? _EndTime;
        #endregion

        public override object Clone()
            => new DecideCampMember(DecideCampGroup);

        public DecideCampGroup DecideCampGroup => Group as DecideCampGroup;
        public double EndTime => _EndTime ?? 0d;

        // must be a member of the team group
        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor?.Adjuncts.OfType<TeamMember>().Any(_tm => _tm.TeamGroup.ID == DecideCampGroup.TeamGroup.ID) ?? false)
            && base.CanAnchor(newAnchor);

        protected override void OnActivate(object source)
        {
            // set an expiration
            _EndTime = Anchor?.GetCurrentTime() + Minute.UnitFactor;
            base.OnActivate(source);
        }

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            // expire if still present
            if (timeVal >= _EndTime)
            {
                Eject();
            }
        }

        public double Resolution => Round.UnitFactor;

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            yield return new CancelStartSetupCamp(DecideCampGroup, @"101");
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Start Camp", ID);
    }
}
