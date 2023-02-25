using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class ObjectInfoVM : ViewModelBase
    {
        public ObjectInfo ObjectInfo { get; set; }
        public Visibility IconVisibility { get; set; }
        public Visibility TextVisibility { get; set; }
        public Orientation StackOrientation { get; set; }
        public double Size { get; set; }
    }
}
