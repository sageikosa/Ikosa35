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
using Uzi.Ikosa.Advancement;
using System.Windows.Controls.Primitives;

namespace Uzi.Ikosa.UI
{
    /// <summary>
    /// Interaction logic for EditFeat.xaml
    /// </summary>
    public partial class EditFeat : UserControl
    {
        public EditFeat()
        {
            InitializeComponent();
        }

        public ItemsControl AdvancementLogControl
        {
            get { return (ItemsControl)GetValue(AdvancementLogControlProperty); }
            set { SetValue(AdvancementLogControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AdvancementLogControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdvancementLogControlProperty =
            DependencyProperty.Register("AdvancementLogControl", typeof(ItemsControl), 
            typeof(EditFeat), new UIPropertyMetadata(null));

        
        
        private void mnuFeatClick(object sender, RoutedEventArgs e)
        {
            var _elem = sender as FrameworkElement;
            var _pd = _elem.Tag as PowerDie;
            var _origSource = e.OriginalSource as FrameworkElement;
            if (_origSource.DataContext is FeatListItem _featItem)
            {
                _pd.Feat = _featItem.Feat;
            }
            else
            {
                var _featParam = _origSource.DataContext as FeatParameter;
                _pd.Feat = _featParam.GetFeat(_featParam.Type, _pd);
            }

            if (AdvancementLogControl != null)
                AdvancementLogControl.Items.Refresh();
        }

        private void ctxPDFeat_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var _stack = sender as Grid;
            var _pd = (DataContext as AdvancementLogItem).PowerDie;
            if (!_pd.IsFeatPowerDie)
            {
                _stack.ContextMenu.IsOpen = false;
                e.Handled = true;
            }
        }

        #region private void stackPDFeat_MouseDown(object sender, MouseButtonEventArgs e)
        private void stackPDFeat_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var _stack = sender as Grid;
                var _pd = (DataContext as AdvancementLogItem).PowerDie;
                if (_pd.IsFeatPowerDie)
                {
                    var _ctx = new ContextMenu
                    {
                        ItemsSource = _pd.AvailableFeats,
                        Tag = _pd
                    };
                    _ctx.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(mnuFeatClick));
                    _ctx.PlacementTarget = _stack;
                    _ctx.Placement = PlacementMode.Bottom;
                    _ctx.IsOpen = true;
                }
                e.Handled = true;
            }
        }
        #endregion
    }
}
