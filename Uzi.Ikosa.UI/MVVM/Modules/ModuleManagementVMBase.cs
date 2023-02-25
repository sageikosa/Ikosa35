using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Uzi.Ikosa.UI
{
    public abstract class ModuleManagementVMBase : INotifyPropertyChanged
    {
        private Commandable _Commands;

        public Commandable Commands
        {
            get => _Commands;
            set
            {
                _Commands = value;
                DoPropertyChanged(nameof(Commands));
            }
        }

        public abstract Commandable GetDefaultCommands();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void DoPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
