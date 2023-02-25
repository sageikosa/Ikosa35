using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class MenuViewModel : MenuBaseViewModel
    {
        public MenuViewModel()
        {
            SubItems = new ObservableCollection<MenuBaseViewModel>();
        }

        #region data
        private Info _Icon;
        private object _Header;
        private object _Parameter;
        private object _ToolTip;
        private bool _IsChecked;
        #endregion

        public bool IsAction { get; set; }

        public object Header
        {
            get { return _Header; }
            set
            {
                _Header = value;
                DoNotify(nameof(Header));
            }
        }

        public Info IconSource
        {
            get { return _Icon; }
            set
            {
                _Icon = value;
                DoNotify(nameof(IconSource));
            }
        }

        public ObservableCollection<MenuBaseViewModel> SubItems { get; set; }
        public ICommand Command { get; set; }

        public object Parameter
        {
            get { return _Parameter; }
            set
            {
                _Parameter = value;
                DoNotify(nameof(Parameter));
            }
        }

        public object ToolTip
        {
            get { return _ToolTip; }
            set
            {
                _ToolTip = value;
                DoNotify(nameof(ToolTip));
            }
        }

        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                _IsChecked = value;
                DoNotify(nameof(IsChecked));
            }
        }

    }
}
