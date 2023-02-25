using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Guildsmanship;

namespace Uzi.Ikosa.UI
{
    public class TeamGroupSummaryVM : ModuleElementVMBase
    {
        private readonly TeamGroupSummaryFolderVM _Folder;

        public TeamGroupSummaryVM(TeamGroupSummaryFolderVM folder, TeamGroupSummary teamGroup)
            : base(teamGroup)
        {
            _Folder = folder;
            Commands = GetDefaultCommands();
        }

        public TeamGroupSummaryFolderVM Folder => _Folder;
        public TeamGroupSummary TeamGroupSummary => Element as TeamGroupSummary;

        public override Module IkosaModule
            => Folder.IkosaModule;

        public IEnumerable<TeamGroupAttitudeVM> Attitudes
            => TeamGroupSummary.Attitudes
            .Select(_kvp => new TeamGroupAttitudeVM(_kvp.Value, Folder.IkosaModule.GetTeamGroupSummary(_kvp.Key)));

        public void RefreshAttitudes() => DoPropertyChanged(nameof(Attitudes));

        public override Commandable GetDefaultCommands()
            => new AddRemoveEditCommands
            {
                AddCommand = new RelayCommand<object>((target) =>
                {
                    Commands = new AddTeamGroupAttitudeCommands(this);
                }),
                EditCommand = new RelayCommand<object>(
                    target =>
                    {
                        if (target is TeamGroupAttitudeVM _tga)
                        {
                            Commands = new EditTeamGroupAttitudeCommands(this, _tga);
                        }
                    },
                    target => target is TeamGroupAttitudeVM _tga),
                RemoveCommand = new RelayCommand<object>(
                    target =>
                    {
                        if (target is TeamGroupAttitudeVM _tga)
                        {
                            TeamGroupSummary.Attitudes.Remove(_tga.TargetTeam.ID);
                            RefreshAttitudes();
                        }
                    },
                    target => target is TeamGroupAttitudeVM _tga)
            };
    }

    public class AddTeamGroupAttitudeCommands : DoAddCommands
    {
        public AddTeamGroupAttitudeCommands(TeamGroupSummaryVM parent)
            : base(parent)
        {
            Description = string.Empty;
            Attitude = Attitude.Unbiased;
            IsConstituted = false;

            // exclude self and any groups already defined
            TargetGroups = parent.Folder.IkosaModule.GetResolvableTeamGroups()
                .Where(_tgs => _tgs.ID != parent.TeamGroupSummary.ID
                && !parent.TeamGroupSummary.Attitudes.ContainsKey(_tgs.ID))
                .OrderBy(_tgs => _tgs.Name)
                .ToList();

            SelectedTeamGroup = TargetGroups.FirstOrDefault();

            DoAddCommand = new RelayCommand(() =>
            {
                parent.TeamGroupSummary.Attitudes.Add(SelectedTeamGroup.ID, new TeamAttitude
                {
                    Attitude = Attitude,
                    Description = Description,
                    IsConstituted = IsConstituted
                });
                parent.RefreshAttitudes();
                parent.Commands = parent.GetDefaultCommands();
            },
            () => !string.IsNullOrWhiteSpace(Description) && (SelectedTeamGroup != null));
        }

        public string Description { get; set; }
        public Attitude Attitude { get; set; }
        public bool IsConstituted { get; set; }

        public IList<TeamGroupSummary> TargetGroups { get; set; }
        public TeamGroupSummary SelectedTeamGroup { get; set; }
    }

    public class EditTeamGroupAttitudeCommands : DoEditCommands
    {
        private readonly TeamGroupAttitudeVM _TGA;

        public EditTeamGroupAttitudeCommands(TeamGroupSummaryVM parent, TeamGroupAttitudeVM attitude)
            : base(parent)
        {
            _TGA = attitude;
            Description = attitude.TeamAttitude.Description;
            Attitude = attitude.TeamAttitude.Attitude;
            IsConstituted = attitude.TeamAttitude.IsConstituted;

            DoEditCommand = new RelayCommand(() =>
            {
                attitude.TeamAttitude.Description = Description;
                attitude.TeamAttitude.Attitude = Attitude;
                attitude.TeamAttitude.IsConstituted = IsConstituted;
                parent.RefreshAttitudes();
                parent.Commands = parent.GetDefaultCommands();
            },
            () => !string.IsNullOrWhiteSpace(Description));
        }

        public TeamGroupAttitudeVM TeamGroupAttitude => _TGA;
        public string Description { get; set; }
        public Attitude Attitude { get; set; }
        public bool IsConstituted { get; set; }
    }
}
