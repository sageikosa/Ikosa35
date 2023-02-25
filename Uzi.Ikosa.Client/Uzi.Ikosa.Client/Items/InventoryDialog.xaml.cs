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
using Uzi.Ikosa.Proxy.IkosaSvc;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Client
{
    /// <summary>
    /// Interaction logic for InventoryDialog.xaml
    /// </summary>
    public partial class InventoryDialog : Window
    {
        public InventoryDialog(ItemSlotInfo[] slots)
        {
            InitializeComponent();
            invAdmin.SetItemSlotInfo(slots);
        }
    }
}
