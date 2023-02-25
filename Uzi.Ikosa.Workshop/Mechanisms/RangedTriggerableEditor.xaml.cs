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
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for RangedTriggerableEditor.xaml
    /// </summary>
    public partial class RangedTriggerableEditor : UserControl
    {
        public RangedTriggerableEditor()
        {
            InitializeComponent();

            _WeaponCmd = new RelayCommand(() =>
            {
                var _dlg = new ObjectEditorWindow(RangedTriggerable)
                {
                    Owner = Window.GetWindow(this),
                    Title = RangedTriggerable?.Thing.Name ?? @"Ranged Trap",
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

        public PresentableThingVM<RangedTriggerable> RangedTriggerable
        {
            get { return (PresentableThingVM<RangedTriggerable>)GetValue(RangedTriggerableProperty); }
            set { SetValue(RangedTriggerableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MeleeTriggerable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RangedTriggerableProperty =
            DependencyProperty.Register(
                nameof(RangedTriggerable),
                typeof(RangedTriggerable),
                typeof(RangedTriggerableEditor),
                new PropertyMetadata(null, new PropertyChangedCallback(OnOpenCloseTriggerableChanged)));

        private static void OnOpenCloseTriggerableChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is RangedTriggerableEditor _editor)
            {
                // ???
            }
        }
    }
}
