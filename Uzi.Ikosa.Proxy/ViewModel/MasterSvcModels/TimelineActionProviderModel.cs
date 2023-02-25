using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class TimelineActionProviderModel : ViewModelBase
    {
        #region data
        private ActionInfo _Selected;
        #endregion

        public ActionProviderInfo ActionProvider { get; set; }
        public List<ActionInfo> Actions { get; set; }

        public ActionInfo SelectedAction
        {
            get => _Selected;
            set
            {
                _Selected = value;
                DoPropertyChanged(nameof(SelectedAction));
            }
        }
    }
}
