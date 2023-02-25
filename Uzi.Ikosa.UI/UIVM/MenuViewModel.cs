using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Uzi.Ikosa.UI
{
    public class MenuViewModel: MenuBaseViewModel
    {
        public MenuViewModel()
        {
            SubItems = new List<object>();
        }

        public bool IsChecked { get; set; }
        public object Header { get; set; }
        public List<object> SubItems { get; set; }
        public ICommand Command { get; set; }
        public object Parameter { get; set; }
        public object ToolTip { get; set; }
    }
}
