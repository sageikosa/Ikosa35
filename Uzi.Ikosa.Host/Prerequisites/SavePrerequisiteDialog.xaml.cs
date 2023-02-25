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
    /// Interaction logic for SavePrerequisiteDialog.xaml
    /// </summary>
    public partial class SavePrerequisiteDialog : Window
    {
        public SavePrerequisiteDialog(SavePrerequisite preReq)
        {
            InitializeComponent();
            DataContext = preReq;
        }

        public SavePrerequisite SavePrerequisite { get { return DataContext as SavePrerequisite; } }

        private void cboRoll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cboRoll != null) && (cboRoll.SelectedItem != null))
                txtValue.Text = (cboRoll.SelectedItem as ComboBoxItem).Content.ToString();
            e.Handled = true;
        }

        private void btnRoll_Click(object sender, RoutedEventArgs e)
        {
            txtValue.Text = DieRoller.RollDie(Guid.Empty, 20, SavePrerequisite.BindKey, SavePrerequisite.Name).ToString();
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

        public int? Value()
        {
            if ((txtValue == null) || (txtValue.Text == null))
                return null;

            if (!Int32.TryParse(txtValue.Text, out var _out))
                return null;

            return _out;
        }

        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Value() != null;
            e.Handled = true;
        }
    }
}
