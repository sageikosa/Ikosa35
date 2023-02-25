using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.UI
{
    public class AddRemoveCommands : Commandable
    {
        public RelayCommand<object> AddCommand { get; set; }
        public RelayCommand<object> RemoveCommand { get; set; }
    }
}
