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
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>Interaction logic for AmmoContainerContentsList.xaml</summary>
    public partial class AmmoContainerContentsList : Window
    {
        public AmmoContainerContentsList()
        {
            InitializeComponent();
        }

        private void lstAmmoSets_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (sender is ListBox _list)
            {
                if (_list.HasItems)
                {
                    _list.SelectedIndex = 0;
                }
            }
        }
    }
}
