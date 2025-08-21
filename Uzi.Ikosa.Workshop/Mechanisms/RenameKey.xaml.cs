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
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for RenameKey.xaml
    /// </summary>
    public partial class RenameKey : Window
    {
        public RenameKey(Guid guid, LocalMap map)
        {
            InitializeComponent();
            if (map.NamedKeyGuids.ContainsKey(guid))
            {
                txtName.Text = map.NamedKeyGuids[guid];
            }

            _Guid = guid;
            _Map = map;
        }

        private Guid _Guid;
        private LocalMap _Map;

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (_Map.NamedKeyGuids.ContainsKey(_Guid))
            {
                _Map.NamedKeyGuids[_Guid] = txtName.Text;
            }
            else
            {
                _Map.NamedKeyGuids.Add(_Guid, txtName.Text);
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
