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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for RoomContentControl.xaml
    /// </summary>
    public partial class RoomContentControl : ContentControl
    {
        public RoomContentControl(LocalMap map, object source)
        {
            InitializeComponent();
            this.Resources.Add(@"roomMaterials", map.AllCellMaterials);
            this.DataContext = source;
            this.Content = source;
        }

        private void txtInt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            int _out = 0;
            if (!int.TryParse(_txt.Text, out _out))
            {
                _txt.Tag = @"Invalid";
                return;
            }
            _txt.Tag = null;
        }

        private void txtDouble_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            double _out = 0;
            if (!double.TryParse(_txt.Text, out _out))
            {
                _txt.Tag = @"Invalid";
                return;
            }
            _txt.Tag = null;
        }

    }
}
