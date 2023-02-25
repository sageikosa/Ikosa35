using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ProximityTriggerMechanismEditor.xaml
    /// </summary>
    public partial class ProximityTriggerMechanismEditor : UserControl
    {
        public ProximityTriggerMechanismEditor()
        {
            InitializeComponent();

            // control is the view model
            DataContext = this;
        }

        public ProximityTriggerMechanism ProximityTriggerMechanism
        {
            get { return (ProximityTriggerMechanism)GetValue(ProximityTriggerMechanismProperty); }
            set { SetValue(ProximityTriggerMechanismProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProximityTriggerMechanismProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProximityTriggerMechanismProperty =
            DependencyProperty.Register(
                nameof(ProximityTriggerMechanism),
                typeof(ProximityTriggerMechanism),
                typeof(ProximityTriggerMechanismEditor),
                new PropertyMetadata(null, new PropertyChangedCallback(OnProximityTriggerMechanismChanged)));

        private static void OnProximityTriggerMechanismChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is ProximityTriggerMechanismEditor _surfTriggerEdit)
            {
                // ???
            }
        }

        public IEnumerable<Size> AvailableSizes
        {
            get
            {
                yield return Size.Tiny;
                yield return Size.Small;
                yield return Size.Medium;
                yield return Size.Large;
                yield return Size.Huge;
                yield break;
            }
        }

        public Size MinimumSize
        {
            get => Size.Medium.OffsetSize(ProximityTriggerMechanism?.MinimumSize ?? 0);
            set
            {
                if (ProximityTriggerMechanism != null)
                    ProximityTriggerMechanism.MinimumSize = value.Order;
            }
        }

        private void btnTargets_Click(object sender, RoutedEventArgs e)
        {
            var _dlg = new Window
            {
                Owner = Window.GetWindow(this),
                Content = new TriggerTargetsEditor { TriggerMechanism = ProximityTriggerMechanism },
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.ToolWindow,
                Title = ProximityTriggerMechanism.Name,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            _dlg.ShowDialog();
        }
    }
}
