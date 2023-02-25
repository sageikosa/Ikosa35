using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.UI
{
    public class DoAddCommands : Commandable
    {
        protected DoAddCommands(ModuleManagementVMBase parent)
        {
            CancelAddCommand = new RelayCommand(() =>
            {
                parent.Commands = parent.GetDefaultCommands();
            });
        }

        public RelayCommand CancelAddCommand { get; set; }
        public RelayCommand DoAddCommand { get; set; }
    }
}
