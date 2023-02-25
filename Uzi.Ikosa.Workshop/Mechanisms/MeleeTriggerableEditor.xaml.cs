using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for MeleeTriggerableEditor.xaml
    /// </summary>
    public partial class MeleeTriggerableEditor : UserControl
    {
        public MeleeTriggerableEditor()
        {
            InitializeComponent();

            _WeaponCmd = new RelayCommand(() =>
            {
                var _dlg = new ObjectEditorWindow(MeleeTriggerable)
                {
                    Owner = Window.GetWindow(this),
                    Title = MeleeTriggerable?.Thing.Name ?? @"Melee Trap",
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                _dlg.ShowDialog();
            });

            // control is the view model
            DataContext = this;
        }

        #region data
        private RelayCommand _WeaponCmd;
        #endregion

        public RelayCommand WeaponCommand => _WeaponCmd;

        public PresentableThingVM<MeleeTriggerable> MeleeTriggerable
        {
            get { return (PresentableThingVM<MeleeTriggerable>)GetValue(MeleeTriggerableProperty); }
            set { SetValue(MeleeTriggerableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MeleeTriggerable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MeleeTriggerableProperty =
            DependencyProperty.Register(
                nameof(MeleeTriggerable),
                typeof(MeleeTriggerable),
                typeof(MeleeTriggerableEditor),
                new PropertyMetadata(null, new PropertyChangedCallback(OnOpenCloseTriggerableChanged)));

        private static void OnOpenCloseTriggerableChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is MeleeTriggerableEditor _editor)
            {
                // ???
            }
        }
    }
}
