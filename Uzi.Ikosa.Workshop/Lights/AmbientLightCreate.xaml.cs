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
    /// Interaction logic for AmbientLightCreate.xaml
    /// </summary>
    public partial class AmbientLightCreate : Window
    {
        public AmbientLightCreate(LocalMap map)
        {
            InitializeComponent();
            _Map = map;
        }

        private LocalMap _Map;

        public AmbientLight CreateAmbientLight()
        {
            try
            {
                // name
                if (string.IsNullOrEmpty(txtName.Text))
                {
                    return null;
                }
                if (_Map.AmbientLights.ContainsKey(txtName.Text))
                {
                    return null;
                }

                // values
                double _zLo = 0;
                double _yLo = 0;
                double _xLo = 0;
                if (!double.TryParse(txtZOffset.Text, out _zLo)
                    || !double.TryParse(txtYOffset.Text, out _yLo)
                    || !double.TryParse(txtXOffset.Text, out _xLo)
                    )
                {
                    return null;
                }

                double _vbr = 0;
                double _br = 0;
                double _nr = 0;
                double _fr = 0;
                if (!double.TryParse(txtVeryBright.Text, out _vbr)
                    || !double.TryParse(txtBright.Text, out _br)
                    || !double.TryParse(txtNear.Text, out _nr)
                    || !double.TryParse(txtFar.Text, out _fr)
                    )
                {
                    return null;
                }
                else if ((_vbr < 0) || (_br < 0) || (_nr < 0) || (_fr < 0))
                {
                    return null;
                }

                var _level = LightRange.OutOfRange;
                if (_vbr > 0)
                    _level = (chkSolar.IsChecked ?? false) ? LightRange.Solar : LightRange.VeryBright;
                else if (_br > 0)
                    _level = LightRange.Bright;
                else if (_nr > 0)
                    _level = LightRange.NearShadow;
                else if (_fr > 0)
                    _level = LightRange.FarShadow;

                // all clear
                return new AmbientLight(_Map, txtName.Text, new System.Windows.Media.Media3D.Vector3D(_xLo, _yLo, _zLo),
                    _level, _vbr, _br, _nr, _fr);
            }
            catch
            {
                return null;
            }
        }

        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                // name
                if (string.IsNullOrEmpty(txtName.Text))
                {
                    txtMessage.Text = @"Name is empty";
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }
                if (_Map.AmbientLights.ContainsKey(txtName.Text))
                {
                    txtMessage.Text = @"Name is not unique";
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }

                // values
                double _zLo = 0;
                double _yLo = 0;
                double _xLo = 0;
                if (!double.TryParse(txtZOffset.Text, out _zLo)
                    || !double.TryParse(txtYOffset.Text, out _yLo)
                    || !double.TryParse(txtXOffset.Text, out _xLo)
                    )
                {
                    txtMessage.Text = @"One or more invalid offsets";
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }

                double _vbr = 0;
                double _br = 0;
                double _nr = 0;
                double _fr = 0;
                if (!double.TryParse(txtVeryBright.Text, out _vbr)
                    || !double.TryParse(txtBright.Text, out _br)
                    || !double.TryParse(txtNear.Text, out _nr)
                    || !double.TryParse(txtFar.Text, out _fr)
                    )
                {
                    txtMessage.Text = @"One or more invalid range offsets";
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }
                else if ((_vbr < 0) || (_br < 0) || (_nr < 0) || (_fr < 0))
                {
                    txtMessage.Text = @"One or more range offsets below 0";
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }

                // all clear
                txtMessage.Text = string.Empty;
                e.CanExecute = true;
                e.Handled = true;
            }
            catch
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }

        private void cmdbndOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void txtVector_LostFocus(object sender, RoutedEventArgs e)
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

        private void txtLevel_LostFocus(object sender, RoutedEventArgs e)
        {
            var _txt = sender as TextBox;
            double _out = 0;
            if (!double.TryParse(_txt.Text, out _out))
            {
                _txt.Tag = @"Invalid";
                return;
            }
            else if (_out < 0)
            {
                _txt.Tag = @"Invalid";
                return;
            }
            _txt.Tag = null;
        }
    }
}
