using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.UI
{
    public class TeamGroupAttitudeVM
    {
        private TeamAttitude _Attitude;
        private TeamGroupSummary _TargetTeam;

        public TeamGroupAttitudeVM(TeamAttitude attitude, TeamGroupSummary targetTeam)
        {
            _Attitude = attitude;
            _TargetTeam = targetTeam;
        }

        public TeamAttitude TeamAttitude => _Attitude;
        public TeamGroupSummary TargetTeam => _TargetTeam;

        public string Constituted => TeamAttitude.IsConstituted ? @"declared" : @"implicitly";
    }
}
