using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace Uzi.Ikosa.UI
{
    /// <summary>
    /// Interaction logic for EditRequirements.xaml
    /// </summary>
    public partial class EditRequirements : UserControl
    {
        public EditRequirements()
        {
            InitializeComponent();
        }

        public ItemsControl AdvancementLogItemsControl
        {
            get { return (ItemsControl)GetValue(AdvancementLogItemsControlProperty); }
            set { SetValue(AdvancementLogItemsControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AdvancementLogItemsControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdvancementLogItemsControlProperty =
            DependencyProperty.Register("AdvancementLogItemsControl", typeof(ItemsControl), typeof(EditRequirements), 
            new UIPropertyMetadata(null));

        private void mnuOptionsClick(object sender, RoutedEventArgs e)
        {
            // TODO: plug parameter into parameterized advancement option (from menu "above" the selected one)
            // TODO: create advancement parameter class, that knows of its parent, handle that in SetBonusFeat...
            // TODO: consider skill-focus\knowledge\arcana (double deep parameters)
            var _elem = sender as FrameworkElement;
            var _origSource = e.OriginalSource as FrameworkElement;
            var _opt = _origSource.DataContext as AdvancementOption;
            _opt.Target.SetOption(_opt);

            // TODO: refresh just the one row, or use property notification...
            AdvancementLogItemsControl.Items.Refresh();
        }

        #region private void txtRequirement_MouseUp(object sender, MouseButtonEventArgs e)
        private void txtRequirement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var _grd = sender as Grid;
                var _req = _grd.Tag as AdvancementRequirement;
                var _ctx = new ContextMenu
                {
                    ItemsSource = _req.AvailableOptions
                };
                _ctx.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(mnuOptionsClick));
                _ctx.PlacementTarget = _grd;
                _ctx.Placement = PlacementMode.Bottom;
                _ctx.IsOpen = true;
                e.Handled = true;
            }
        }
        #endregion
    }
}
