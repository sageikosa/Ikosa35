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
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Host.Prerequisites
{
    /// <summary>
    /// Interaction logic for ChoicePrerequisiteDialog.xaml
    /// </summary>
    public partial class ChoicePrerequisiteDialog : Window
    {
        public ChoicePrerequisiteDialog(ChoicePrerequisite preReq)
        {
            InitializeComponent();
            this.DataContext = preReq;
        }

        public ChoicePrerequisite ChoicePrerequisite { get { return DataContext as ChoicePrerequisite; } }

        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (cboChoice != null) && (cboChoice.SelectedItem != null);
            e.Handled = true;
        }

        public OptionAimOption SelectedOption { get { return cboChoice.SelectedItem as OptionAimOption; } }

        private void cmdbndOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _opt = cboChoice.SelectedItem as OptionAimOption;
            if (_opt != null)
            {
                this.DialogResult = true;
                this.Close();
            }
            e.Handled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
            e.Handled = true;
        }
    }
}
