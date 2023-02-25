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
using System.Windows.Shapes;
using Uzi.Ikosa.Advancement;
using System.Windows.Controls.Primitives;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for MultiRequirementsEditor.xaml
    /// </summary>
    public partial class MultiRequirementsEditor : Window
    {
        public MultiRequirementsEditor()
        {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(MultiRequirementsEditor_DataContextChanged);
        }

        void MultiRequirementsEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _logItem = LogItem;
            if (_logItem != null)
            {
                Title = string.Format(@"Requirements for {0} ({1})", _logItem.AdvancementClass.ClassName, _logItem.AdvancementClassLevelHigh);
            }
        }

        private AdvancementLogItem LogItem { get { return DataContext as AdvancementLogItem; } }

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

        private void mnuOptionsClick(object sender, RoutedEventArgs e)
        {
            // TODO: plug parameter into parameterized advancement option (from menu "above" the selected one)
            // TODO: create advancement parameter class, that knows of its parent, handle that in SetBonusFeat...
            // TODO: consider skill-focus\knowledge\arcana (double deep parameters)
            FrameworkElement _elem = sender as FrameworkElement;
            FrameworkElement _origSource = e.OriginalSource as FrameworkElement;
            AdvancementOption _opt = _origSource.DataContext as AdvancementOption;
            _opt.Target.SetOption(_opt);

            // hard refresh
            icRequirements.ItemsSource = null;
            icRequirements.ItemsSource = LogItem.Requirements;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
