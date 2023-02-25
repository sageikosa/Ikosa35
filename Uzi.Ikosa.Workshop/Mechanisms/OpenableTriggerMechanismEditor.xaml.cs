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
    /// Interaction logic for OpenableTriggerMechanismEditor.xaml
    /// </summary>
    public partial class OpenableTriggerMechanismEditor : UserControl
    {
        public OpenableTriggerMechanismEditor()
        {
            InitializeComponent();

            // control is the view model
            DataContext = this;
        }

        public OpenableTriggerMechanism OpenableTriggerMechanism
        {
            get { return (OpenableTriggerMechanism)GetValue(OpenableTriggerMechanismProperty); }
            set { SetValue(OpenableTriggerMechanismProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpenableTriggerMechanismProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenableTriggerMechanismProperty =
            DependencyProperty.Register(
                nameof(OpenableTriggerMechanism),
                typeof(OpenableTriggerMechanism),
                typeof(OpenableTriggerMechanismEditor),
                new PropertyMetadata(null, new PropertyChangedCallback(OnOpenableTriggerMechanismChanged)));

        private static void OnOpenableTriggerMechanismChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is OpenableTriggerMechanismEditor _openTriggerEdit)
            {
                // ???
            }
        }

        private void btnTargets_Click(object sender, RoutedEventArgs e)
        {
            var _dlg = new Window
            {
                Owner = Window.GetWindow(this),
                Content = new TriggerTargetsEditor { TriggerMechanism = OpenableTriggerMechanism },
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.ToolWindow,
                Title = OpenableTriggerMechanism.Name,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            _dlg.ShowDialog();
        }
    }
}
