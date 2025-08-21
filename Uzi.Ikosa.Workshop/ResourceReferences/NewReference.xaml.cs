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

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for NewReference.xaml
    /// </summary>
    public partial class NewReference : Window
    {
        public NewReference()
        {
            InitializeComponent();
        }

        public NewReference(Func<string, bool> isValid)
        {
            InitializeComponent();
            _IsValid = isValid;
        }

        private Func<string, bool> _IsValid;

        public string ReferenceName { get { return txtName.Text; } }

        private void cbSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_IsValid != null)
            {
                e.CanExecute = _IsValid(txtName.Text);
            }

            e.Handled = true;
        }

        private void cbSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true;
            e.Handled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
