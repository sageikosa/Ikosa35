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
using Uzi.Ikosa.Actions;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Host.Prerequisites
{
    /// <summary>
    /// Interaction logic for CheckPrerequisiteDialog.xaml
    /// </summary>
    public partial class CheckPrerequisiteDialog : Window
    {
        public CheckPrerequisiteDialog(SuccessCheckPrerequisite preReq)
        {
            InitializeComponent();
            if (preReq.Check.VoluntaryPenalty != 0)
                chkPenalty.Visibility = System.Windows.Visibility.Visible;
            else
                chkPenalty.Visibility = System.Windows.Visibility.Collapsed;
            DataContext = preReq;
        }

        public SuccessCheckPrerequisite SuccessCheckPrerequisite => DataContext as SuccessCheckPrerequisite;

        private void cboRoll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cboRoll != null) && (cboRoll.SelectedItem != null))
                txtValue.Text = (cboRoll.SelectedItem as ComboBoxItem).Content.ToString();
            e.Handled = true;
        }

        private void btnRoll_Click(object sender, RoutedEventArgs e)
        {
            txtValue.Text = DieRoller.RollDie(Guid.Empty, 20, SuccessCheckPrerequisite.BindKey, SuccessCheckPrerequisite.Name).ToString();
            e.Handled = true;
        }

        public int? Value()
        {
            if ((txtValue == null) || (txtValue.Text == null))
                return null;

            if (!Int32.TryParse(txtValue.Text, out var _out))
                return null;

            return _out;
        }

        public bool IsUsingPenalty => chkPenalty.IsChecked ?? false;

        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Value() != null;
            e.Handled = true;
        }

        private void cmdbndOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
            Close();
            e.Handled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
            e.Handled = true;
        }
    }
}
