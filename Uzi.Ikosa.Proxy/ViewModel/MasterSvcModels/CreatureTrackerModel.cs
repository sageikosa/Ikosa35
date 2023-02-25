using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class CreatureTrackerModel : ViewModelBase
    {
        public CreatureTrackerInfo CreatureTrackerInfo { get; set; }
        public bool IsInTracker { get; set; }
        public bool NeedsTurnTick { get; set; }
        public bool IsInitiative { get; set; }
        public Guid ID => CreatureTrackerInfo?.ID ?? Guid.Empty;
        public bool HasUser => CreatureTrackerInfo?.UserInfos.Any() ?? false;
        public bool HasMasterUser => CreatureTrackerInfo?.UserInfos.Any(_ui => _ui.IsMaster) ?? false;
        public bool IsExpanded { get; set; }
        public TickTrackerModeBase TickTrackerMode { get; set; }
        public ViewModelBase Group { get; set; }

        public Visibility IsFocusedVisibility
            => (CreatureTrackerInfo?.LocalActionBudgetInfo?.IsFocusedBudget ?? false)
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility HeldActivityDescriptionVisibility
            => !string.IsNullOrWhiteSpace(CreatureTrackerInfo?.LocalActionBudgetInfo?.HeldActivity?.Description)
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility HeldActivityVisibility
            => (CreatureTrackerInfo?.LocalActionBudgetInfo?.HeldActivity != null)
            ? Visibility.Visible
            : Visibility.Collapsed;

        public virtual void Conformulate(CreatureTrackerModel conform)
        {
            CreatureTrackerInfo = conform.CreatureTrackerInfo;
            IsInTracker = conform.IsInTracker;
            NeedsTurnTick = conform.NeedsTurnTick;
            IsInitiative = conform.IsInitiative;
            TickTrackerMode = conform.TickTrackerMode;
            Group = conform.Group;
            DoPropertyChanged(nameof(CreatureTrackerInfo));
            DoPropertyChanged(nameof(IsInTracker));
            DoPropertyChanged(nameof(NeedsTurnTick));
            DoPropertyChanged(nameof(IsInitiative));
            DoPropertyChanged(nameof(HasUser));
            DoPropertyChanged(nameof(HasMasterUser));
            DoPropertyChanged(nameof(TickTrackerMode));
            DoPropertyChanged(nameof(Group));
            DoPropertyChanged(nameof(HeldActivityVisibility));
            DoPropertyChanged(nameof(HeldActivityDescriptionVisibility));
        }

        // TODO: distance from master locus
    }
}
