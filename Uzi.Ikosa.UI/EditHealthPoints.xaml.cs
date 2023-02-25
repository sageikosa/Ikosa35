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
    /// Interaction logic for EditHealthPoints.xaml
    /// </summary>
    public partial class EditHealthPoints : UserControl
    {
        public EditHealthPoints()
        {
            InitializeComponent();
        }

        public static RoutedCommand HPRoll = new RoutedCommand();
        public static RoutedCommand HPAverage = new RoutedCommand();
        public static RoutedCommand HPMax = new RoutedCommand();

        private void cbHPRoll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbHPRoll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AdvancementLogItem _logItem = e.Parameter as AdvancementLogItem;
            _logItem.PowerDie.RerollHealthPoints();
        }

        private void cbHPAverage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbHPAverage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AdvancementLogItem _logItem = e.Parameter as AdvancementLogItem;
            _logItem.PowerDie.AverageHealthPoints();
        }

        private void cbHPMax_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;

        }

        private void cbHPMax_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AdvancementLogItem _logItem = e.Parameter as AdvancementLogItem;
            _logItem.PowerDie.MaxHealthPoints();
        }

        #region private void lblHPValue_MouseUp(object sender, MouseButtonEventArgs e)
        private void lblHPValue_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var _pop = ((sender as TextBlock).Tag as Popup);
                _pop.IsOpen = true;
                _pop.Child.Focus();
                e.Handled = true;
            }
        }
        #endregion
    }
}
