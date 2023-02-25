using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for OpenCloseTriggerableEditor.xaml
    /// </summary>
    public partial class OpenCloseTriggerableEditor : UserControl
    {
        public OpenCloseTriggerableEditor()
        {
            InitializeComponent();

            // control is the view model
            DataContext = this;
        }

        public OpenCloseTriggerable OpenCloseTriggerable
        {
            get { return (OpenCloseTriggerable)GetValue(OpenCloseTriggerableProperty); }
            set { SetValue(OpenCloseTriggerableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpenableTriggerMechanismProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenCloseTriggerableProperty =
            DependencyProperty.Register(
                nameof(OpenCloseTriggerable),
                typeof(OpenCloseTriggerable),
                typeof(OpenCloseTriggerableEditor),
                new PropertyMetadata(null, new PropertyChangedCallback(OnOpenCloseTriggerableChanged)));

        private static void OnOpenCloseTriggerableChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is OpenCloseTriggerableEditor _triggerable)
            {
                // ???
            }
        }
    }
}
