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
using Uzi.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for IkosaFolderRename.xaml
    /// </summary>
    public partial class IkosaFolderRename : Window
    {
        public IkosaFolderRename(CorePackagePartsFolder folder)
        {
            InitializeComponent();
            txtName.Text = folder.Name; ;
            txtName.SelectAll();
        }
        public string FolderName { get { return txtName.Text; } }

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
