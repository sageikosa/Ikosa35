using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class WorkflowStep : ViewModelBase
    {
        private string _Description;

        public string Description
        {
            get => _Description;
            set
            {
                _Description = value;
                DoPropertyChanged(nameof(Description));
            }
        }
    }
}
