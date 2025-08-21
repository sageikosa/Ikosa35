using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Uzi.Core;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for SwitchActivationMechanismEditor.xaml
    /// </summary>
    public partial class SwitchActivationMechanismEditor : UserControl
    {
        public SwitchActivationMechanismEditor()
        {
            InitializeComponent();

            // control is the view model
            DataContext = this;
        }

        public SwitchActivationMechanism SwitchActivationMechanism
        {
            get { return (SwitchActivationMechanism)GetValue(SwitchActivationMechanismProperty); }
            set { SetValue(SwitchActivationMechanismProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SwitchActivationMechanism.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SwitchActivationMechanismProperty =
            DependencyProperty.Register(
                nameof(SwitchActivationMechanism),
                typeof(SwitchActivationMechanism),
                typeof(SwitchActivationMechanismEditor),
                new PropertyMetadata(null, new PropertyChangedCallback(OnSwitchActivationMechanismChanged)));

        public double OpenState
        {
            get => SwitchActivationMechanism?.OpenState.Value ?? 0;
            set
            {
                if (SwitchActivationMechanism != null)
                {
                    SwitchActivationMechanism.OpenState = new OpenStatus(value);
                }
            }
        }

        private static void OnSwitchActivationMechanismChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is SwitchActivationMechanismEditor _switchActEdit)
            {
                // ???
            }
        }

        private void btnTargets_Click(object sender, RoutedEventArgs e)
        {
            var _dlg = new Window
            {
                Owner = Window.GetWindow(this),
                Content = new ActivationTargetsEditor { ActivationMechanism = SwitchActivationMechanism },
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.ToolWindow,
                Title = SwitchActivationMechanism.Name,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            _dlg.ShowDialog();
        }
    }
}
