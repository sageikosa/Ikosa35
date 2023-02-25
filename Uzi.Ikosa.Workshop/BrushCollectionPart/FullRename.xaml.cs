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
    /// Interaction logic for FullRename.xaml
    /// </summary>
    public partial class FullRename : Window
    {
        public FullRename(string name)
        {
            InitializeComponent();
            txtCurrent.Text = name;
            txtCurrent.Tag = name;
        }

        public string GetName()
        {
            return txtCurrent.Text;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
            e.Handled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            txtCurrent.Text = txtCurrent.Tag.ToString();
            this.DialogResult = false;
            this.Close();
            e.Handled = true;
        }

    }
}
