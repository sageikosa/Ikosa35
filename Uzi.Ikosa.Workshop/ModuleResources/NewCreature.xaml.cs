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
    /// Interaction logic for NewCreature.xaml
    /// </summary>
    public partial class NewCreature : Window
    {
        public NewCreature(ModuleResources manager)
        {
            InitializeComponent();
            txtName.SelectAll();
            _Manager = manager;
            _OKCommand = new RelayCommand(() =>
            {
                DialogResult = true;
            },
            () => !string.IsNullOrWhiteSpace(CreatureName) && createCreature.CanCreate && _Manager.CanUseName(CreatureName, typeof(ModuleNode)));
            btnOK.Command = _OKCommand;
            DataContext = _Manager.Module;
        }

        private ModuleResources _Manager;
        private RelayCommand _OKCommand;

        public string CreatureName => txtName.Text;

        public Creature GetCreature() 
            => createCreature.GetCreature(CreatureName);

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
