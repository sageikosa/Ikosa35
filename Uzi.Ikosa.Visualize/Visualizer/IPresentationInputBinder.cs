using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Uzi.Visualize
{
    public interface IPresentationInputBinder
    {
        IEnumerable<InputBinding> GetBindings(Presentable presentable);
    }
}
