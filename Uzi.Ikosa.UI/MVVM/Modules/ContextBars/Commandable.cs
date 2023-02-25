using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Uzi.Ikosa.UI
{
    public abstract class Commandable : INotifyPropertyChanged
    {
        public ModuleManagementVMBase Owner { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void DoPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
