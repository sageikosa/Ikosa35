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
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.UI
{
    /// <summary>Interaction logic for RenameDialog.xaml</summary>
    public partial class RenameDialog : Window
    {
        public RenameDialog(string name)
        {
            InitializeComponent();
            txtName.Text = name;
        }

        public string RenamingName { get { return txtName.Text; } }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
