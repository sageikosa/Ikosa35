using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class PrerequisiteModel : INotifyPropertyChanged
    {
        #region data
        private bool _IsSent = false;
        #endregion

        public bool IsSingleton { get; set; }
        public IPrerequisiteProxy PrerequisiteProxy { get; set; }
        public PrerequisiteInfo Prerequisite { get; set; }
        public CreatureLoginInfo Fulfiller { get; set; }

        public static PrerequisiteModel ToModel(PrerequisiteInfo info,
            IPrerequisiteProxy preReqProxy,
            List<AwarenessInfo> awarenesses,
            Dictionary<Guid, CreatureLoginInfo> resolveFulfillers)
        {
            if (info is OpportunisticPrerequisiteInfo)
            {
                return new OpportunisticPrerequisiteModel
                {
                    PrerequisiteProxy = preReqProxy,
                    Prerequisite = info,
                    IsSingleton = false,
                    Fulfiller = resolveFulfillers != null
                        ? (resolveFulfillers.ContainsKey(info.FulfillerID) ? resolveFulfillers[info.FulfillerID] : null)
                        : null
                };
            }
            else if (info is PromptTurnTrackerPrerequisiteInfo)
            {
                return new PromptTurnTrackerPrerequisiteModel
                {
                    PrerequisiteProxy = preReqProxy,
                    Prerequisite = info,
                    IsSingleton = false,
                    Fulfiller = resolveFulfillers != null
                        ? (resolveFulfillers.ContainsKey(info.FulfillerID) ? resolveFulfillers[info.FulfillerID] : null)
                        : null
                };
            }
            else if (info is CoreSelectPrerequisiteInfo _coreSelect)
            {
                var _model = new CoreSelectPrerequisiteModel
                {
                    PrerequisiteProxy = preReqProxy,
                    Prerequisite = info,
                    IsSingleton = false,
                    Fulfiller = resolveFulfillers != null
                        ? (resolveFulfillers.ContainsKey(info.FulfillerID) ? resolveFulfillers[info.FulfillerID] : null)
                        : null
                };
                _model.SetInfos(_coreSelect.IDs, awarenesses);
                return _model;
            }
            else
            {
                return new PrerequisiteModel
                {
                    PrerequisiteProxy = preReqProxy,
                    Prerequisite = info,
                    IsSingleton = false,
                    Fulfiller = resolveFulfillers != null
                        ? (resolveFulfillers.ContainsKey(info.FulfillerID) ? resolveFulfillers[info.FulfillerID] : null)
                        : null
                };
            }
        }

        public bool IsSent
        {
            get { return _IsSent; }
            set
            {
                _IsSent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSent)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Background)));
            }
        }

        public Brush Background
            => IsSent ? Brushes.Aquamarine : null;

        public Visibility SingletonVisibility
            => IsSingleton ? Visibility.Visible : Visibility.Collapsed;

        public Visibility MultipleVisibility
            => IsSingleton ? Visibility.Collapsed : Visibility.Visible;

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
