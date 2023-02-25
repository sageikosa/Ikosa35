using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.UI
{
    public abstract class DoEditCommands : Commandable
    {
        protected DoEditCommands(ModuleManagementVMBase cancelTarget)
        {
            CancelEditCommand = new RelayCommand(() => cancelTarget.Commands = cancelTarget.GetDefaultCommands());
        }

        public RelayCommand CancelEditCommand { get; set; }
        public RelayCommand DoEditCommand { get; set; }
    }
}
