using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for LockActivationMechanismEditor.xaml
    /// </summary>
    public partial class LockActivationMechanismEditor : UserControl
    {
        public LockActivationMechanismEditor()
        {
            InitializeComponent();

            _Lock = new RelayCommand(() => LockActivationMechanism?.SecureLock(null, null, true));
            _Unlock = new RelayCommand(() => LockActivationMechanism?.UnsecureLock(null, null, true));

            // control is the view model
            DataContext = this;
        }

        #region data
        private RelayCommand _Lock;
        private RelayCommand _Unlock;
        #endregion

        public LockActivationMechanism LockActivationMechanism
        {
            get { return (LockActivationMechanism)GetValue(LockActivationMechanismProperty); }
            set { SetValue(LockActivationMechanismProperty, value); }
        }

        public RelayCommand LockCommand => _Lock;
        public RelayCommand UnlockCommand => _Unlock;

        // Using a DependencyProperty as the backing store for LockActivationMechanism.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LockActivationMechanismProperty =
            DependencyProperty.Register(
                nameof(LockActivationMechanism),
                typeof(LockActivationMechanism),
                typeof(LockActivationMechanismEditor),
                new PropertyMetadata(null, new PropertyChangedCallback(OnLockActivationMechanismChanged)));

        private static void OnLockActivationMechanismChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is LockActivationMechanismEditor _lockActEdit)
            {
                // ???
            }
        }

        private void btnKeySelect_Click(object sender, RoutedEventArgs e)
        {
            if (LockActivationMechanism != null)
            {
                var _selector = new KeySelector(LockActivationMechanism)
                {
                    Owner = Window.GetWindow(this)
                };
                _selector.ShowDialog();
            }
        }

        private void btnTargets_Click(object sender, RoutedEventArgs e)
        {
            var _dlg = new Window
            {
                Owner = Window.GetWindow(this),
                Content = new ActivationTargetsEditor { ActivationMechanism = LockActivationMechanism },
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.ToolWindow,
                Title = LockActivationMechanism.Name,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            _dlg.ShowDialog();
        }
    }
}
