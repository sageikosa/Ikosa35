using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.UI
{
    public class EditCommands : Commandable
    {
        public RelayCommand<object> EditCommand { get; set; }
    }
}
