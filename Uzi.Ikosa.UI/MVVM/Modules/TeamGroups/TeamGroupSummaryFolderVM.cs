using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Guildsmanship;
using Uzi.Packaging;

namespace Uzi.Ikosa.UI
{
    public class TeamGroupSummaryFolderVM : ModuleManagementVMBase
    {
        private readonly Module _Module;
        private readonly PartsFolder _Folder;
        private TeamGroupSummaryVM _Selected;

        public TeamGroupSummaryFolderVM(Module module, PartsFolder folder)
        {
            _Module = module;
            _Folder = folder;
            Commands = GetDefaultCommands();
        }

        // TODO: observable collection
        public IEnumerable<TeamGroupSummaryVM> TeamGroups
            => _Folder.FolderContents.OfType<TeamGroupSummary>()
            .Select(_tgs => new TeamGroupSummaryVM(this, _tgs));

        public Module IkosaModule => _Module;
        public PartsFolder PartsFolder => _Folder;

        public TeamGroupSummaryVM SelectedTeamGroup
        {
            get => _Selected;
            set
            {
                if (_Selected != null)
                    _Selected.Commands = _Selected.GetDefaultCommands();
                _Selected = value;
                DoPropertyChanged(nameof(SelectedTeamGroup));
            }
        }

        public void RefreshTeamGroups()
        {
            PartsFolder.ContentsChanged();
            DoPropertyChanged(nameof(TeamGroups));
        }

        public override Commandable GetDefaultCommands()
            => new AddRemoveCommands
            {
                AddCommand = new RelayCommand<object>((target) =>
                {
                    Commands = new AddTeamGroupSummaryCommands(this);
                }),
                RemoveCommand = new RelayCommand<object>((target) =>
                {
                    if (target is TeamGroupSummaryVM _tgs)
                    {
                        IkosaModule.Teams.Remove(_tgs.TeamGroupSummary.ID);
                        RefreshTeamGroups();
                    }
                })
            };
    }

    public class AddTeamGroupSummaryCommands : DoAddCommands
    {
        public AddTeamGroupSummaryCommands(TeamGroupSummaryFolderVM parent)
            : base(parent)
        {
            DoAddCommand = new RelayCommand(() =>
            {
                var _tgs = new TeamGroupSummary(TeamGroupName);
                parent.IkosaModule.Teams.Add(_tgs.ID, _tgs);
                parent.RefreshTeamGroups();
                parent.Commands = parent.GetDefaultCommands();
                parent.PartsFolder.ContentsChanged();
            },
            () => !string.IsNullOrWhiteSpace(TeamGroupName) && parent.IkosaModule.CanUseName(TeamGroupName, typeof(TeamGroupSummary)));
        }
        public string TeamGroupName { get; set; }
    }
}
