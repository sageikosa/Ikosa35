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
    /// Interaction logic for SelectRename.xaml
    /// </summary>
    public partial class SelectRename : Window
    {
        public SelectRename(string current, IList<string> choices)
        {
            InitializeComponent();
            lstKeys.ItemsSource = choices;
            txtCurrent.Text = current;
        }

        public string GetName()
        {
            if ((lstKeys != null) && (lstKeys.SelectedItem != null))
            {
                return lstKeys.SelectedItem.ToString();
            }

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
            this.DialogResult = false;
            this.Close();
            e.Handled = true;
        }

        private void lstKeys_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.DialogResult = (lstKeys != null) && (lstKeys.SelectedItem != null);
            this.Close();
            e.Handled = true;
        }
    }
}
