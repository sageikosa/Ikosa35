using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Uzi.Ikosa.Guildsmanship;
using Uzi.Ikosa.UI;
using Uzi.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for NewName.xaml
    /// </summary>
    public partial class NewName : Window
    {
        public NewName(string title, string tryName, ICorePartNameManager manager)
        {
            InitializeComponent();
            Title = title;
            txtName.Text = tryName;
            txtName.SelectAll();
            _Manager = manager;
            _OKCommand = new RelayCommand(() =>
            {
                DialogResult = true;
            },
            () => !string.IsNullOrWhiteSpace(ReturnedName) && _Manager.CanUseName(ReturnedName, typeof(ModuleNode)));
            btnOK.Command = _OKCommand;
        }

        private ICorePartNameManager _Manager;
        private RelayCommand _OKCommand;

        public string ReturnedName => txtName.Text;

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
